(function(){
	var incMenu = jN.menu.addMenu('Includes');

	var fso = new ActiveXObject("Scripting.FileSystemObject");
	var incDirObj = fso.GetFolder(jN.includeDir);

	var openFile = function(){
		Editor.open(this.path);
	}

    incMenu.addItem({
        text: 'Run File',
        cmd: function(){
            addScript(Editor.currentView.text);
        }
    });

    incMenu.addItem({
        path: jN.includeDir,
        text: 'Open Script Folder',
        cmd: function() {
            var objShell = new ActiveXObject("Shell.Application");
            objShell.open(this.path);
        }
    });

    incMenu.addItem({
        text: 'API Document',
        cmd: function() {
            var objShell = new ActiveXObject("Shell.Application");
            objShell.open(Editor.nppDir + "\\Plugins\\jN\\doc\\jNAPI.htm");
        }
    });

    incMenu.addSeparator();

	if (incDirObj){
		var filesEnum = new Enumerator(incDirObj.files);
		for (; !filesEnum.atEnd(); filesEnum.moveNext()){
			var file = filesEnum.item().Path;
			if (/\.js$/i.test(file)){
				incMenu.addItem({
					path:file,
					text:filesEnum.item().Name,
					cmd:openFile
				});
			}
		}
	}
})();