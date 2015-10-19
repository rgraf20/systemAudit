<?php

/**
	Controlador que s'encarrega d'enviar informació a javascript depenent de la clau rebuda per post.
*/

#region HEADERS
header('Content-Type: text/html; charset=utf-8');
#endregion HEADERS

#region INCLUDES
include_once('../models/db.php');
include_once('../services/wol_shut.php');
#endregion INCLUDES

#region MAIN
//Instanciem el model(connectem a la BD).
$db = new DB();
session_start();
if(isset($_POST['checkLogin'])) //Login check.
	echo $db->checkLogin($_POST['mail'], $_POST['pass']);
else if(isset($_POST["logout"])) //Tanquem sessió.
	session_destroy();
else
{
	if(isset($_SESSION["logged"]))
	{
		if(isset($_POST['allSystems'])) //Tots els sistemes.
			echo $db->getAllSystems();
		else if(isset($_POST['oneSystem'])) //Un sistema en concret.
			echo $_POST['mac'] == "" ? "dummy" : $db->getOneSystem($_POST['mac']);
		else if(isset($_POST['statusAll'])) //Status de tots els sistemes.
			echo $_POST['macs'] == "" ? "dummy" : $db->getStatusAll($_POST['macs']);
		else if(isset($_POST['status'])) //Status de un sistema.
			echo $_POST['mac'] == "" ? "dummy" : $db->getStatus($_POST['mac']);
		else if(isset($_POST['shut'])) //Shutdown.
			echo shutdown($_POST['userName'], $_POST['pass'], $_POST['hostname']);
		else if(isset($_POST['wol'])) //WOL.
			echo wol($_POST['mac'], $_POST['broad']);
		else if(isset($_POST['addUser'])) //Add user.
			echo $db->addUser($_POST['userName'], $_POST['mail'], $_POST['pass']);
	}
	else
		echo "not logged";
}
$db->close();
#endregion MAIN

?>