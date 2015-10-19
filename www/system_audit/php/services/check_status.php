<?php

/**
	Servei que checkeja els ordinadors de la xarxa i introdueïx si els PC's estàn online o no a la BD.
*/

#region HEADERS
header('Content-Type: text/html; charset=utf-8');
#endregion HEADERS

#region INCLUDES
include_once('/var/www/system_audit/php/utils/magic_constants.php');
include_once('/var/www/system_audit/php/models/db.php');
#endregion INCLUDES

#region MAIN
//Instanciem el model(connectem a la BD).
$db = new DB();
checkStatus($db);
$db->close();
#endregion FUNCTIONS

#region FUNCTIONS
//S'encarrega de chekejar i updatar tots els sistemes de la xarxa.
function checkStatus($db)
{
	$result = $db->returnAllIps();
	while($row = mysqli_fetch_assoc($result)) //Per cada ip fem ping i un update del status.
	{
		$ip = $row["ip"];
		$mac = $row["mac_address"];
		$status = shell_exec("ping -W 1 -c 1 -q $ip >/dev/null 2>&1 && echo ONLINE || echo OFFLINE");
		$status = trim($status, "\n");
		$status = trim($status, "\t");
		$status = str_replace(' ', '', $status);
		echo $status . "\n";
		echo $db->executeSentence("UPDATE system SET status='$status' WHERE mac_address='$mac';");
	}	
}
#endregion FUNCTIONS

?>

