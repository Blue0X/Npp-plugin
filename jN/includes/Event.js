var eventListener = {
    FILESAVED: function(v) {
        //自动去掉行尾空格
        MenuCmds.EDIT_TRIMTRAILING();
        MenuCmds.FILE_SAVE();
    }
}
GlobalListener.addListener(eventListener);
