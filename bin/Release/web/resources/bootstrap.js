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
    this.connection = new connection(this.url);
    this.connection.on('@login', function (payload) {
      this.me = { name: this.name, id: payload.connectionId }
      this.loginStatus = 1;
    }, this);

    this.connection.on('close', () => this.loginStatus = 0);
    this.connection.on('error', () => this.loginStatus = 0);
  },
  methods: {
    login () {
      if (this.connection.status !== 1) {
        this.connection.on('connected', function () {
          this.sendLogin()
        }, this);

        if (this.connection.status == 0) {
          this.connection.connect()
        }
        return;
      }
      this.sendLogin()
    },
    sendLogin () {
      this.connection.send('login', { name: this.name });
    },
    post () {
      this.connection.send('post', { message: this.message });
    },
    quit () {
      this.connection.quit();
    }
  }
});