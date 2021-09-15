new Vue({
  el: '#app',
  data () {
    return {
      url: 'ws://127.0.0.1:4189/',
      me: null,
      loginHandler: null,
      name: null,
      connection: null,
      loginStatus: 0,
      message: ''
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

    conn.on('@enter', (payload) => console.log('用户进入聊天', payload));
    conn.on('@exit', (payload) => console.log('用户退出聊天', payload));

    conn.on('@post', (payload) => {
      console.log('新消息', payload)
    })
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