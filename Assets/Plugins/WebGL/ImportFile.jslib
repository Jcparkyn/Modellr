mergeInto(LibraryManager.library, {
	ImportFile : function()
	{
		document.addEventListener('click', function() {

			var fileuploader = document.getElementById('fileuploader');
			if (!fileuploader) {
				fileuploader = document.createElement('input');
				fileuploader.setAttribute('style','display:none;');
				fileuploader.setAttribute('type', 'file');
				fileuploader.setAttribute('id', 'fileuploader');
				fileuploader.setAttribute('class', 'focused');
				document.getElementsByTagName('body')[0].appendChild(fileuploader);

				fileuploader.onchange = function(e) {
					var files = e.target.files;
					var reader = new FileReader();
					reader.onload = function(e) {
						return e.target.result;
					}
					reader.readAsText(files[0]);
					//for (var i = 0, f; f = files[i]; i++) {
					//	window.alert(URL.createObjectURL(f));
					//	SendMessage('" + gameObject.name +@"', 'FileDialogResult', URL.createObjectURL(f));
					//}
				};
			}
			if (fileuploader.getAttribute('class') == 'focused') {
				fileuploader.setAttribute('class', '');
				fileuploader.click();
			}
		});    
	}
});