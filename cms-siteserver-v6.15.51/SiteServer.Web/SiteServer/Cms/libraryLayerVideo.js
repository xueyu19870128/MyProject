﻿var $urlUploadVideo = apiUrl + '/pages/cms/libraryLayerVideo/actions/uploadVideo?siteId=' + utils.getQueryInt('siteId');
var $urlUploadImage = apiUrl + '/pages/cms/libraryLayerVideo/actions/uploadImage?siteId=' + utils.getQueryInt('siteId');

var data = {
  pageLoad: false,
  pageAlert: null,
  activeName: 'first',
  form: {
    siteId: utils.getQueryInt('siteId'),
    type: 'upload',
    videoUrl: '',
    isPoster: false,
    isAutoPlay: false,
    isWidth: false,
    isHeight: false,
    imageUrl: '',
    width: '100%',
    height: '300px',
    isLinkToOriginal: true,
  },
  player: null
};

var methods = {
  btnTabsClick: function() {
    if (this.activeName !== 'second') return;
    if (this.player) {
      this.player.poster(this.form.isPoster ? this.form.imageUrl : '');
      this.player.src([{src: this.form.videoUrl}]);
    } else {
      this.player = videojs(this.$refs.videoPlayer, {
        poster: this.form.isPoster ? this.form.imageUrl : '',
        sources: [
          {
            src: this.form.videoUrl
          }
        ]
      });
    }
  },

  btnSubmitClick: function () {
    var $this = this;

    if (!this.form.videoUrl) {
      this.$message.error('请设置需要插入的视频文件！');
      return false;
    }

    var imageUrl = this.form.isPoster && this.form.imageUrl ? ' imageUrl="' + this.form.imageUrl + '"' : '';
    var isAutoPlay = ' isAutoPlay="' + this.form.isAutoPlay + '"';
    var width = this.form.isWidth ? ' width="' + this.form.width + '"' : '';
    var height = this.form.isHeight ? ' height="' + this.form.height + '"' : '';

    parent.insertHtml('<img ' + imageUrl + isAutoPlay + width + height + ' playUrl="' + this.form.videoUrl + '" class="siteserver-stl-player" style="width: 333px; height: 333px" src="../assets/ueditor/video-clip.png" /><br/>');
    parent.layer.closeAll();
  },

  btnCancelClick: function () {
    parent.layer.closeAll();
  },

  uploadVideoBefore(file) {
    var re = /(\.mp4|\.flv|\.f4v|\.webm|\.m4v|\.mov|\.3gp|\.3g2)$/i;
    if(!re.exec(file.name))
    {
      this.$message.error('文件只能是视频格式，请选择有效的文件上传!');
      return false;
    }
    return true;
  },

  uploadImageBefore(file) {
    var re = /(\.jpg|\.jpeg|\.bmp|\.gif|\.png|\.webp)$/i;
    if(!re.exec(file.name))
    {
      this.$message.error('文件只能是图片格式，请选择有效的文件上传!');
      return false;
    }

    var isLt10M = file.size / 1024 / 1024 < 10;
    if (!isLt10M) {
      this.$message.error('上传图片大小不能超过 10MB!');
      return false;
    }
    return true;
  },

  uploadProgress: function() {
    utils.loading(true)
  },

  uploadVideoSuccess: function(res) {
    this.form.videoUrl = res.url;
    this.form.type = 'url';
    utils.loading(false);
  },

  uploadImageSuccess: function(res) {
    this.form.imageUrl = res.url;
    utils.loading(false);
  },

  uploadError: function(err) {
    utils.loading(false);
    var error = JSON.parse(err.message);
    this.$message.error(error.message);
  }
};

new Vue({
  el: '#main',
  data: data,
  methods: methods,
  created: function () {
    this.pageLoad = true;
  }
});