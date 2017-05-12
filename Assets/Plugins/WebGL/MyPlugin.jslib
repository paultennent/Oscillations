var MyPlugin = {
    GetFilenames: function()
    {   
	    
    	var url = location.href;
    	var name = 'rn';
    	name = name.replace(/[\[]/,"\\\[").replace(/[\]]/,"\\\]");
    	var regexS = "[\\?&]"+name+"=([^&#]*)";
    	var regex = new RegExp( regexS );
    	var results = regex.exec( url );
        var returnStr = "0000";

        if(results){
            returnStr = results[1];
        }

        var buffer = _malloc(lengthBytesUTF8(returnStr) + 1);
        writeStringToMemory(returnStr, buffer);
        return buffer;
    },
};

mergeInto(LibraryManager.library, MyPlugin);