(function(){
    //rss源
    var rssSources = {
        1: {
            rss: 'http://feed.qiushibaike.com/rss',
            cmd: function(xmlHttp){
                var content = xmlHttp.responseXML.selectNodes('/rss/channel/item/description');
                showContent('糗事百科', content[0].text);
            }
        },
        2: {
            rss: 'http://www.qiyu.net/rss',
            cmd: function(xmlHttp) {
                var titles = xmlHttp.responseXML.selectNodes('/rss/channel/item/title');
                var html = '<p><select style="width:100%" onchange="Dialog.cfg.showRssItem(this.value, Dialog)">';
                var descriptions = xmlHttp.responseXML.selectNodes('/rss/channel/item/description');
                for(var i = 0, c = titles.length; i < c; i++){
                    html += '<option value="' + i +'">' + titles[i].text + '</option>';
                }
                html += '</select></p>';
                html += '<div id="content">' + descriptions[0].text + '</div>';
                var dlg = new Dialog({
                    npp:Editor,
                    title: '奇遇',
                    css: 'div, p{font-size:12px;}',
                    html: html,
                    width:640,
                    clientWidth:640,
                    height:300,
                    clientHeight:300,
                    showRssItem: function(i, dialog) {
                       var descriptions = xmlHttp.responseXML.selectNodes('/rss/channel/item/description');
                       dialog.htmlDialog.document.getElementById('content').innerHTML = descriptions[i].text;
                    }
                });
                dlg.show();
            }
        }
    }

    //菜单
    var rssMenu = jN.menu.addMenu('RSS');
    rssMenu.addItem({
        text: '糗事百科',
        id: 1,
        cmd: fetchContent
    });
    rssMenu.addItem({
        text: '奇遇',
        id: 2,
        cmd: fetchContent
    });

    //取内容
    function fetchContent() {
        var xmlHttp = new ActiveXObject("Microsoft.XMLHTTP");
        if (!xmlHttp) return;
        var rssSource = rssSources[this.id];
        xmlHttp.open('GET', rssSource.rss, true);
        xmlHttp.onreadystatechange = function() {
            if (xmlHttp.readyState == 4 && xmlHttp.responseXML) {
                rssSource.cmd(xmlHttp);
            }
        };
        xmlHttp.send(null);
    }

    //显示内容
    function showContent(title, content) {
        var dlg = new Dialog({
            npp:Editor,
            title: title,
            css: 'div, p{font-size:12px;}',
            html: content,
            width:640,
            clientWidth:640,
            height:300,
            clientHeight:300
        });
        dlg.show();
    }

})();