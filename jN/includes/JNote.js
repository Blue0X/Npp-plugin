(function(){
    var jNoteDlg = null;
    jN.menu.addItem({
        text: 'JNote备忘',
        cmd:function(){
            if (!jNoteDlg) {
                var content = readFile(jN.includeDir + "JNote");
                jNoteDlg = new Dialog({
                    npp:Editor,
                    css: "body{overflow:hidden;margin:0;padding:0;} textarea {width:100%;height:200px;background-color: #293134;color:#ffffff;}",
                    dockable: {
                            name: 'jNote',
                            docking: "bottom",
                            onbeforeclose: function() {
                                jNoteDlg = null;
                            }
                        },
                    html: "<textarea id='jnote' onkeyup='Dialog.cfg.jNoteKeyPress(window.event, Dialog)'>" + content + "</textarea>",
                    height:200,
                    clientHeight:200,
                    title: "Escape key to save and close",
                    jNoteKeyPress: function(evt, dialog) {
                        var target = evt.srcElement || evt.target,
                        keycode = evt.keyCode || evt.which;
                        if (keycode == 27) {
                            writeFile(jN.includeDir + 'JNote', target.value);
                            dialog.hide();
                        }
                    }
                });
            }
            jNoteDlg.show();
        }
    });
})();