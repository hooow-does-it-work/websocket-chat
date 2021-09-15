function connection (wsUrl) {
  this.wsUrl = wsUrl;
  this.socket = null;
  this.autoReconnect = true;
  this.status = 0;
  this.connectFailedCount = 0;
  this.__events = {};
}

// 简单的事件注册。
connection.prototype.on = function (ev, handler, once, context) {
  this.__events[ev] = { handler, once: once === true, context: context || null };
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

connection.prototype.reconnect = function () {
  if (this.status !== 0 || this.connectFailedCount >= 10) return;
  setTimeout(() => this.connect(), 1500);
}

connection.prototype.send = function (action, payload) {
  if (this.status !== 2) return;
  this.socket.send(JSON.stringify({ action, payload }));
}
connection.prototype.quit = function () {
  if (this.status !== 2) return;
  this.socket.send(JSON.stringify({ action: 'quit', payload: {} }));
}
// 连接
connection.prototype.connect = function () {
  this.status = 1;
  const that = this
  const webSocket = new WebSocket(this.wsUrl);
  webSocket.onopen = function () {
    that.socket = webSocket;
    that.status = 2
    that.connectFailedCount = 0;
    that.emit('connected');
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
    if (that.autoReconnect) that.reconnect();
  }

  webSocket.onerror = function () {
    that.status = 0;
    that.emit('error', ex);
    if (that.autoReconnect) that.reconnect();
  }
}