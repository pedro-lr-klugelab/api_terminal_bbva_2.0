$( document ).ajaxStart(function() {
	$('.progress-bar').css('width', '100%').attr('aria-valuenow', 100);
	$("#loader").show();
});

$( document ).ajaxStop(function() {
  $('.progress-bar').css('width', '0%').attr('aria-valuenow', 0);
  $("#loader").hide();
});

$(document).ready(function(){
	$("#loader").hide();
});