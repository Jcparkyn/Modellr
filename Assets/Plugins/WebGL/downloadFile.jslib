mergeInto(LibraryManager.library, {
	DownloadFile : function(array, size, fileNamePtr)
	{
		var fileName = UTF8ToString(fileNamePtr);
	 
		var bytes = new Uint8Array(size);
		for (var i = 0; i < size; i++)
		{
		   bytes[i] = HEAPU8[array + i];
		}
	 
		var blob = new Blob([bytes]);
		var link = document.createElement('a');
		link.href = window.URL.createObjectURL(blob);
		link.download = fileName;
	 
		var event = document.createEvent("MouseEvents");
		event.initMouseEvent("click");
		link.dispatchEvent(event);
		window.URL.revokeObjectURL(link.href);
		//link.parentNode.removeChild(link);
	},
	
	RequestFile : function(){
		var fileuploader = document.getElementById('fileuploader');
		if (!fileuploader) {
			fileuploader = document.createElement('input');
			fileuploader.setAttribute('style','display:none;');
			fileuploader.setAttribute('type', 'file');
			//fileuploader.setAttribute('accept', '.obj');
			fileuploader.setAttribute('id', 'fileuploader');
			fileuploader.setAttribute('class', 'focused');
			document.getElementsByTagName('body')[0].appendChild(fileuploader);

			fileuploader.onchange = function(e) {
				var files = e.target.files;
				for (var i = 0, f; f = files[i]; i++) {
					//window.alert(URL.createObjectURL(f));
					unityInstance.SendMessage('Mesh', 'FileDialogResult', URL.createObjectURL(f));
				}
			};
		}
		if (fileuploader.getAttribute('class') == 'focused') {
			fileuploader.setAttribute('class', '');
			fileuploader.click();
		}
	}
});