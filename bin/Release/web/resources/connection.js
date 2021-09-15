function connection (wsUrl) {
  this.wsUrl = wsUrl;
  this.socket = null;
  this.status = 0;
  this.__events = {};
}

// 简单的事件注册。
connection.prototype.on = function (ev, handler, context) {
  this.__events[ev] = { handler, once: false, context: context || null };
}
connection.prototype.once = function (ev, handler, context) {
  this.__events[ev] = { handler, once: true, context: context || null };
}

// 调用事件
connection.prototype.emit = function (ev, ...args) {
  if (!this.__events[ev]) return;
  const handler = this.__events[ev];
  handler.handler.apply(handler.context, args);
  if (handler.once === true) {
    this.__events[ev] = null;
  }
};

connection.prototype.send = function (action, payload) {
  if (this.status !== 2) return;
  this.socket.send(JSON.stringify({ action, payload }));
}

connection.prototype.quit = function () {
  if (this.status !== 2) return;
  this.socket.send(JSON.stringify({ action: 'quit', payload: {} }));
}

connection.prototype.login = function (name) {
  if (this.status !== 2) {
    this.once('wait-connected', () => this.send('login', { name: name }));
    if (this.status === 0) this.connect();
    return;
  }
  this.send('login', { name: name });
};


// 连接
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

  webSocket.onerror = function () {
    that.status = 0;
    that.emit('error', ex);
  }
}