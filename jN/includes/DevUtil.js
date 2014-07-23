(function(){
    //cssbeautify & cssmin
    function cssbeautify(style,opt){'use strict';var options,index=0,length=style.length,blocks,formatted='',ch,ch2,str,state,State,depth,quote,comment,openbracesuffix=true,trimRight;options=arguments.length>1?opt:{};if(typeof options.indent==='undefined'){options.indent='    '}if(typeof options.openbrace==='string'){openbracesuffix=(options.openbrace==='end-of-line')}function isWhitespace(c){return(c===' ')||(c==='\n')||(c==='\t')||(c==='\r')||(c==='\f')}function isQuote(c){return(c==='\'')||(c==='"')}function isName(c){return(ch>='a'&&ch<='z')||(ch>='A'&&ch<='Z')||(ch>='0'&&ch<='9')||'-_*.:#'.indexOf(c)>=0}function appendIndent(){var i;for(i=depth;i>0;i-=1){formatted+=options.indent}}function openBlock(){formatted=trimRight(formatted);if(openbracesuffix){formatted+=' {'}else{formatted+='\n';appendIndent();formatted+='{'}if(ch2!=='\n'){formatted+='\n'}depth+=1}function closeBlock(){depth-=1;formatted=trimRight(formatted);formatted+='\n';appendIndent();formatted+='}';blocks.push(formatted);formatted=''}if(String.prototype.trimRight){trimRight=function(s){return s.trimRight()}}else{trimRight=function(s){return s.replace(/\s+$/,'')}}State={Start:0,AtRule:1,Block:2,Selector:3,Ruleset:4,Property:5,Separator:6,Expression:7};depth=0;state=State.Start;comment=false;blocks=[];style=style.replace(/\r\n/g,'\n');while(index<length){ch=style.charAt(index);ch2=style.charAt(index+1);index+=1;if(isQuote(quote)){formatted+=ch;if(ch===quote){quote=null}if(ch==='\\'&&ch2===quote){formatted+=ch2;index+=1}continue}if(isQuote(ch)){formatted+=ch;quote=ch;continue}if(comment){formatted+=ch;if(ch==='*'&&ch2==='/'){comment=false;formatted+=ch2;index+=1}continue}else{if(ch==='/'&&ch2==='*'){comment=true;formatted+=ch;formatted+=ch2;index+=1;continue}}if(state===State.Start){if(blocks.length===0){if(isWhitespace(ch)&&formatted.length===0){continue}}if(ch<=' '||ch.charCodeAt(0)>=128){state=State.Start;formatted+=ch;continue}if(isName(ch)||(ch==='@')){str=trimRight(formatted);if(str.length===0){if(blocks.length>0){formatted='\n\n'}}else{if(str.charAt(str.length-1)==='}'||str.charAt(str.length-1)===';'){formatted=str+'\n\n'}else{while(true){ch2=formatted.charAt(formatted.length-1);if(ch2!==' '&&ch2.charCodeAt(0)!==9){break}formatted=formatted.substr(0,formatted.length-1)}}}formatted+=ch;state=(ch==='@')?State.AtRule:State.Selector;continue}}if(state===State.AtRule){if(ch===';'){formatted+=ch;state=State.Start;continue}if(ch==='{'){openBlock();state=State.Block;continue}formatted+=ch;continue}if(state===State.Block){if(isName(ch)){str=trimRight(formatted);if(str.length===0){if(blocks.length>0){formatted='\n\n'}}else{if(str.charAt(str.length-1)==='}'){formatted=str+'\n\n'}else{while(true){ch2=formatted.charAt(formatted.length-1);if(ch2!==' '&&ch2.charCodeAt(0)!==9){break}formatted=formatted.substr(0,formatted.length-1)}}}appendIndent();formatted+=ch;state=State.Selector;continue}if(ch==='}'){closeBlock();state=State.Start;continue}formatted+=ch;continue}if(state===State.Selector){if(ch==='{'){openBlock();state=State.Ruleset;continue}if(ch==='}'){closeBlock();state=State.Start;continue}formatted+=ch;continue}if(state===State.Ruleset){if(ch==='}'){closeBlock();state=State.Start;if(depth>0){state=State.Block}continue}if(ch==='\n'){formatted=trimRight(formatted);formatted+='\n';continue}if(!isWhitespace(ch)){formatted=trimRight(formatted);formatted+='\n';appendIndent();formatted+=ch;state=State.Property;continue}formatted+=ch;continue}if(state===State.Property){if(ch===':'){formatted=trimRight(formatted);formatted+=': ';state=State.Expression;if(isWhitespace(ch2)){state=State.Separator}continue}if(ch==='}'){closeBlock();state=State.Start;if(depth>0){state=State.Block}continue}formatted+=ch;continue}if(state===State.Separator){if(!isWhitespace(ch)){formatted+=ch;state=State.Expression;continue}if(isQuote(ch2)){state=State.Expression}continue}if(state===State.Expression){if(ch==='}'){closeBlock();state=State.Start;if(depth>0){state=State.Block}continue}if(ch===';'){formatted=trimRight(formatted);formatted+=';\n';state=State.Ruleset;continue}formatted+=ch;continue}formatted+=ch}formatted=blocks.join('')+formatted;return formatted}function cssmin(css,linebreakpos){var startIndex=0,endIndex=0,iemac=false,preserve=false,i=0,max=0,preservedTokens=[],token='';css=css.replace(/("([^\\"]|\\.|\\)*")|('([^\\']|\\.|\\)*')/g,function(match){var quote=match[0];preservedTokens.push(match.slice(1,-1));return quote+"___YUICSSMIN_PRESERVED_TOKEN_"+(preservedTokens.length-1)+"___"+quote});while((startIndex=css.indexOf("/*",startIndex))>=0){preserve=css.length>startIndex+2&&css[startIndex+2]==='!';endIndex=css.indexOf("*/",startIndex+2);if(endIndex<0){if(!preserve){css=css.slice(0,startIndex)}}else if(endIndex>=startIndex+2){if(css[endIndex-1]==='\\'){css=css.slice(0,startIndex)+"/*\\*/"+css.slice(endIndex+2);startIndex+=5;iemac=true}else if(iemac&&!preserve){css=css.slice(0,startIndex)+"/**/"+css.slice(endIndex+2);startIndex+=4;iemac=false}else if(!preserve){css=css.slice(0,startIndex)+css.slice(endIndex+2)}else{token=css.slice(startIndex+3,endIndex);preservedTokens.push(token);css=css.slice(0,startIndex+2)+"___YUICSSMIN_PRESERVED_TOKEN_"+(preservedTokens.length-1)+"___"+css.slice(endIndex);if(iemac)iemac=false;startIndex+=2}}}css=css.replace(/\s+/g," ");css=css.replace(/(^|\})(([^\{:])+:)+([^\{]*\{)/g,function(m){return m.replace(":","___YUICSSMIN_PSEUDOCLASSCOLON___")});css=css.replace(/\s+([!{};:>+\(\)\],])/g,'$1');css=css.replace(/___YUICSSMIN_PSEUDOCLASSCOLON___/g,":");css=css.replace(/:first-(line|letter)({|,)/g,":first-$1 $2");css=css.replace(/\*\/ /g,'*/');css=css.replace(/^(.*)(@charset "[^"]*";)/gi,'$2$1');css=css.replace(/^(\s*@charset [^;]+;\s*)+/gi,'$1');css=css.replace(/\band\(/gi,"and (");css=css.replace(/([!{}:;>+\(\[,])\s+/g,'$1');css=css.replace(/;+}/g,"}");css=css.replace(/([\s:])(0)(px|em|%|in|cm|mm|pc|pt|ex)/gi,"$1$2");css=css.replace(/:0 0 0 0;/g,":0;");css=css.replace(/:0 0 0;/g,":0;");css=css.replace(/:0 0;/g,":0;");css=css.replace(/background-position:0;/gi,"background-position:0 0;");css=css.replace(/(:|\s)0+\.(\d+)/g,"$1.$2");css=css.replace(/rgb\s*\(\s*([0-9,\s]+)\s*\)/gi,function(){var rgbcolors=arguments[1].split(',');for(var i=0;i<rgbcolors.length;i++){rgbcolors[i]=parseInt(rgbcolors[i],10).toString(16);if(rgbcolors[i].length===1){rgbcolors[i]='0'+rgbcolors[i]}}return'#'+rgbcolors.join('')});css=css.replace(/([^"'=\s])(\s*)#([0-9a-f])([0-9a-f])([0-9a-f])([0-9a-f])([0-9a-f])([0-9a-f])/gi,function(){var group=arguments;if(group[3].toLowerCase()===group[4].toLowerCase()&&group[5].toLowerCase()===group[6].toLowerCase()&&group[7].toLowerCase()===group[8].toLowerCase()){return(group[1]+group[2]+'#'+group[3]+group[5]+group[7]).toLowerCase()}else{return group[0].toLowerCase()}});css=css.replace(/[^\};\{\/]+\{\}/g,"");if(linebreakpos>=0){startIndex=0;i=0;while(i<css.length){if(css[i++]==='}'&&i-startIndex>linebreakpos){css=css.slice(0,i)+'\n'+css.slice(i);startIndex=i}}}css=css.replace(/;;+/g,";");for(i=0,max=preservedTokens.length;i<max;i++){css=css.replace("___YUICSSMIN_PRESERVED_TOKEN_"+i+"___",preservedTokens[i])}css=css.replace(/^\s+|\s+$/g,"");return css;}

    function UrlFormat() {
        var selection = Editor.currentView.selection, selected = true;
        if (!selection) {
            selection = Editor.currentView.text;
            selected = false;
        }
        selection = selection.replace(/&amp;/g, '&');
        selection = unescape(selection).replace(/\?|&/g, "\r\n");

        if (selected) {
            Editor.currentView.selection = selection;
        }
        else {
            Editor.currentView.text = selection;
        }
    }

    function XMLFormat() {
        var selection = Editor.currentView.selection, selected = true;
        if (!selection) {
            selection = Editor.currentView.text;
            selected = false;
        }
        var xmlObj = new ActiveXObject('Microsoft.XMLDOM');
        xmlObj.loadXML(selection);

        selection = getXMLString(xmlObj.documentElement, 0);
        if (selected) {
            Editor.currentView.selection = selection;
        }
        else {
            Editor.currentView.text = selection;
        }
    }

    function getXMLString(tree, level) {
        var formatted = '';
        var pad = '';

        for (var i = 0; i < level; i++) {
            pad += '    ';
        }
        if(tree.hasChildNodes()) {
            formatted += pad + '<' + tree.tagName + '>\r\n';
            level++;
            var nodes = tree.childNodes.length;
            for (var i = 0; i < nodes; i++) {
                formatted += getXMLString(tree.childNodes(i), level);
            }
            formatted += pad + '</' + tree.tagName + '>\r\n';
        }
        else {
            formatted =  pad + tree.text + "\r\n";
        }
        return formatted;
    }

    var iBoxAPI = {
        js: 'http://tool.lu/js/ajax.html',
        css: 'http://tool.lu/css/ajax.html',
        html: 'http://tool.lu/html/ajax.html',
        php: 'http://tool.lu/php/ajax.html',
        sql: 'http://tool.lu/sql/ajax.html'
    };

    function iBoxService(api, operate) {
        if (!iBoxAPI[api]) return '';

        var xmlHttp = new ActiveXObject("Microsoft.XMLHTTP");
        if (!xmlHttp) return;

        var data = 'operate=' + operate + '&code=';

        if (Editor.currentView.selection) {
            data += encodeURIComponent(Editor.currentView.selection);
        }
        else {
            data += encodeURIComponent(Editor.currentView.text);
        }

        xmlHttp.open('POST', iBoxAPI[api], true);
        xmlHttp.setRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
        xmlHttp.setRequestHeader("Host", "tool.lu");
        xmlHttp.setRequestHeader("Referer", "http://tool.lu/js/");
        xmlHttp.onreadystatechange = function() {
            if (xmlHttp.readyState == 4 && xmlHttp.responseText) {
                try {
                    var result = eval( '(' + xmlHttp.responseText + ')');
                    if (result && result.status) {
                        if (Editor.currentView.selection) {
                            Editor.currentView.selection = result.text;
                        }
                        else {
                            Editor.currentView.text = result.text;
                        }
                    }
                    else {
                        Editor.alert('Error #1');
                    }
                }
                catch(e) {
                    Editor.alert('Error #2');
                }
            }
        };

        xmlHttp.send(data);
    }

    var menu = jN.menu.addMenu('DevUtil');
    menu.addItem({text: 'URL Format', cmd: UrlFormat});
    menu.addItem({text: 'XML Format', cmd: XMLFormat});

    menu.addItem({
        text: 'iBox - JS 美化',
        cmd: function() {
            iBoxService('js', 'beauty');
        }
    });

    menu.addItem({
        text: 'iBox - JS 净化',
        cmd: function() {
            iBoxService('js', 'purify');
        }
    });

    menu.addItem({
        text: 'iBox - JS 加密',
        cmd: function() {
            iBoxService('js', 'pack');
        }
    });

    menu.addItem({
        text: 'iBox - JS 解密',
        cmd: function() {
            iBoxService('js', 'unpack');
        }
    });

    menu.addItem({
        text: 'iBox - JS 混淆',
        cmd: function() {
            iBoxService('js', 'uglify');
        }
    });

    menu.addItem({
        text: 'iBox - CSS 美化',
        cmd: function() {
            Editor.currentView.text = cssbeautify(Editor.currentView.text);
        }
    });

    menu.addItem({
        text: 'iBox - CSS 净化',
        cmd: function() {
            Editor.currentView.text = cssmin(Editor.currentView.text, 0);
        }
    });

    menu.addItem({
        text: 'iBox - CSS 整理',
        cmd: function() {
            iBoxService('css', 'comb');
        }
    });

    menu.addItem({
        text: 'iBox - HTML 美化',
        cmd: function() {
            iBoxService('html', 'beauty');
        }
    });

    menu.addItem({
        text: 'iBox - PHP 美化',
        cmd: function() {
            iBoxService('php', 'beauty');
        }
    });

    menu.addItem({
        text: 'iBox - SQL 美化',
        cmd: function() {
            iBoxService('sql', 'beauty');
        }
    });

})();