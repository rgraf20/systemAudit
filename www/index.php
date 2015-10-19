<?php 

/**
	Redireccionament del localhost.
*/

echo "Redirecting... <br/>";
$ip = $_SERVER['SERVER_ADDR'];
$url = "Location: http://$ip/system_audit/html/";
header($url);
exit;
return;

?>