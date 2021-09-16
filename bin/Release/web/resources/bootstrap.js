/**
 * 防抖函数
 * @param {Function} fn 
 * @param {Number} timeout 
 * @returns 
 */
function lazyFunction (fn, timeout) {
  var timer = 0;
  return function () {
    if (timer) window.clearTimeout(timer);
    var args = arguments, that = this;
    timer = window.setTimeout(function () {
      fn.apply(that, args)
    }, timeout);
  };
}

/**
 * 实例化Vue
 */
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
  watch: {
    messages () {
      this.updateScroll();
    }
  },
  created () {
    /**
     * 初始化connection，注册各种事件
     * @ 开头的事件为服务器发送的消息
     */
    const conn = this.connection = new connection(this.url);
    conn.on('connecting', () => this.loginStatus = 1);
    conn.on('close', () => this.loginStatus = 0);
    conn.on('error', () => this.loginStatus = 0);

    conn.on('@login', function (payload) {
      this.me = { name: this.name, id: payload.connectionId }
      this.loginStatus = 2;
    }, this);

    conn.on('@enter', (payload) => this.messages.push({ type: 'log', message: `${payload.name} 进入聊天室` }), this);
    conn.on('@exit', (payload) => this.messages.push({ type: 'log', message: `${payload.name} 离开聊天室` }), this);

    conn.on('@post', (payload) => this.messages.push({ type: 'post', payload }), this)

  },
  methods: {
    /**
     * 更新滚动条
     */
    updateScroll: lazyFunction(function () {
      this.$nextTick(() => {
        const contentsRef = this.$refs['contents']
        console.log(contentsRef)
        contentsRef.scrollTop = contentsRef.scrollHeight
      });
    }, 30),

    /**
     * 设置样式
     * @param {Object} msg 
     * @returns 
     */
    getClass (msg) {
      if (msg.type === 'log') return 'message-type-log';
      return [
        'message-type-' + msg.type,
        'message-owner-' + (msg.payload.connectionId === this.me.id ? 'mine' : 'user')
      ].join(' ')
    },

    /**
     * 登录
     * @returns 
     */
    login () {
      if (!this.name) {
        return;
      }
      this.connection.login(this.name);
    },

    /**
     * 发布消息
     * @returns 
     */
    post () {
      if (!this.message) return;
      this.connection.send('post', { message: this.message });
      this.message = '';
    },

    /**
     * 退出
     */
    quit () {
      this.connection.quit();
    }
  },
  computed: {

    /**
     * 渲染消息条目
     * @returns 
     */
    contents () {
      const me = this.me;
      return {
        props: {
          msg: { type: Object, required: true }
        },
        render (h) {
          if (this.msg.type === 'log') {
            return h('span', [this.msg.message]);
          }
          return [
            h('div',
              {
                'class': 'message-content'
              },
              [
                h('label', [this.msg.payload.connectionId === me.id ? '我' : this.msg.payload.name]),
                h('div', [this.msg.payload.message])
              ])
          ];
        }
      }
    }
  }
});