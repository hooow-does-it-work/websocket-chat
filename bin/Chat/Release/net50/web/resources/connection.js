/**
 * 管理websocket连接
 * @param {String} wsUrl websocket地址
 */
function connection (wsUrl) {
  this.wsUrl = wsUrl;
  this.socket = null;
  this.status = 0;
  this.__events = {};
}

/**
 * 简单的事件注册。
 * @param {String} ev 
 * @param {Function} handler 
 * @param {any} context 
 */
connection.prototype.on = function (ev, handler, context) {
  this.__events[ev] = { handler, once: false, context: context || this };
}

/**
 * 注册一次性事件
 * @param {String} ev 
 * @param {Function} handler 
 * @param {any} context 
 */
connection.prototype.once = function (ev, handler, context) {
  this.__events[ev] = { handler, once: true, context: context || this };
}

/**
 * 调用事件
 * @param {String} ev 
 * @param  {...any} args 
 * @returns 
 */
connection.prototype.emit = function (ev, ...args) {
  if (!this.__events[ev]) return;
  const handler = this.__events[ev];
  handler.handler.apply(handler.context, args);
  if (handler.once === true) {
    this.__events[ev] = null;
  }
};

/**
 * 发送消息
 * @param {String} action 
 * @param {Object} payload 
 * @returns 
 */
connection.prototype.send = function (action, payload) {
  if (this.status !== 2) return;
  this.socket.send(JSON.stringify({ action, payload }));
}

/**
 * 退出
 * @returns 
 */
connection.prototype.quit = function () {
  if (this.status !== 2) return;
  this.socket.send(JSON.stringify({ action: 'quit', payload: {} }));
}

/**
 * 登录
 * @param {String} name 用户名
 * @returns 
 */
connection.prototype.login = function (name) {
  if (this.status !== 2) {
    this.once('wait-connected', () => this.send('login', { name: name }));
    if (this.status === 0) this.connect();
    return;
  }
  this.send('login', { name: name });
};

/**
 * 连接服务器
 */
connection.prototype.connect = function () {
  this.status = 1;
  const that = this
  const webSocket = new WebSocket(this.wsUrl);
  that.emit('connecting');
  webSocket.onopen = function () {
    that.socket = webSocket;
    that.status = 2
    that.connectFailedCount = 0;
    that.emit('connected');
    that.emit('wait-connected');
  }

  webSocket.onmessage = function (ev) {
    try {
      const payload = JSON.parse(ev.data)

      that.emit('@' + payload.action, payload.payload);
    } catch (ex) {

    }
  };

  webSocket.onclose = function (ev) {
    that.status = 0;
    that.emit('close', ev);
  }

  webSocket.onerror = function (ev) {
    that.status = 0;
    that.emit('error', ev);
  }
}