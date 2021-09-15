new Vue({
  el: '#app',
  data () {
    return {
      url: 'ws://127.0.0.1:4189/',
      me: null,
      loginHandler: null,
      name: 'anlige',
      connection: null,
      loginStatus: 0,
      message: '',
      messages: []
    }
  },
  created () {
    const conn = this.connection = new connection(this.url);
    conn.on('connecting', () => this.loginStatus = 1);
    conn.on('close', () => this.loginStatus = 0);
    conn.on('error', () => this.loginStatus = 0);

    conn.on('@login', function (payload) {
      this.me = { name: this.name, id: payload.connectionId }
      this.loginStatus = 2;
    }, this);

    conn.on('@enter', (payload) => this.messages.push({type: 'log', message: `${payload.name} 进入聊天室。`}), this);
    conn.on('@exit', (payload) => this.messages.push({type: 'log', message: `${payload.name} 退出聊天室。`}), this);

    conn.on('@post', (payload) =>this.messages.push({type: 'post', payload}), this)

    this.login()
  },
  methods: {
    login () {
      this.connection.login(this.name);
    },
    post () {
      this.connection.send('post', { message: this.message });
    },
    quit () {
      this.connection.quit();
    }
  }
});