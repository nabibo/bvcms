function setCookie(name, value, seconds)
{
	if (seconds) {
		var date = new Date();
		date.setTime(date.getTime() + (seconds * 1000));
		var expires = "; expires=" + date.toGMTString();
	}
	else var expires = "";
	document.cookie = name + "=" + value + expires + "; path=/";
}

function getCookie(name)
{
	var nameEQ = name + "=";
	var ca = document.cookie.split(';');
	for (var i = 0; i < ca.length; i++) {
		var c = ca[i];
		while (c.charAt(0) == ' ') c = c.substring(1, c.length);
		if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
	}
	return null;
}

var activeTutorial = "";

function startTutorial(id) {
	$(".joyride-tip-guide").remove();
	if ($(id).attr("continues") == "yes") {
		activeTutorial = id;
		$(this).joyride({ tipContent: id, postRideCallback: queueNextTutorial, postCancelCallback: clearTutorial });
	}
	else {
		$(this).joyride({ tipContent: id, postRideCallback: clearTutorial, postCancelCallback: clearTutorial });
	}
}

function queueNextTutorial() {
	setCookie("tutorialActive", activeTutorial, 30);
}

function resumeTutorial() {
	var id = getCookie("tutorialActive");
	if (id == null) return;
	startTutorial(id);
}

function clearTutorial() {
	setCookie("tutorialActive", "", -120);
}

$(document).ready(function () {
	resumeTutorial();
})