<?php

/**
	Controlador que s'encarrega de introduïr un nou sistema a la BD.
*/

#region HEADERS
header('Content-Type: text/html; charset=utf-8');
#endregion HEADERS

#region INCLUDES
include_once('../utils/magic_constants.php');
include_once('../models/db.php');
#endregion INCLUDES

#region MAIN
//Agafem el sistema que rebem com a XML.
$newSystem = simplexml_load_file('php://input');

//Instanciem el model(connectem a la BD).
$db = new DB();
$mac = "";

//Afegim el sistema.
addAdminIfNeeds($db);
addSystem($db, $newSystem);
$db->close();
#endregion MAIN

#region FUNCTIONS
//S'encarrega d'afegir un sistema i tots els seus components.
function addSystem($db, $newSystem)
{
	$mac = '';
	for($i = 0; $i < sizeof(MagicConstants::$COMPONENTS_ARRAY); $i++) //Per cada component.
	{
		$component = MagicConstants::$COMPONENTS_ARRAY[$i];

		//Si és system agafem la mac per un cop. 
		if($component === "system") 
		{ 
			//Guardem el fitxer xml amb el nom de la mac del sistema a afegir.
			$mac = $db->checkId($newSystem->$component->mac);
			
			//Agafem el sistema que rebem com a fitxer i el guardem amb la seva mac com a nom.
			$newSystemFile = fopen('php://input', 'rb');
			file_put_contents("../../xml_systems/$mac.xml", $newSystemFile);
			
			//Si ja existeix eliminem el sistema i tots els seus components gràcies al ON DELETE CASCADE.
			if ($db->checkIfSystemExists($mac) != 0) 
				echo $db->deleteSystem($mac);
		} 

		$arrayValues = array();
		foreach($newSystem->$component->children() as $children) //Per cada fill afegim el seu texte a un array.
		{
			if($component === "sw") //Si es SW afegirem a la taula les dades dels programes.
			{
				$arrayValues = array();
				foreach($children->children() as $subChildren) //Per cada program(subnode).
					array_push($arrayValues, $subChildren);
				echo $db->insertComponent("installed_sw", $arrayValues, $mac);
			}
			//Sinó a les corresponents taules.
			else
			{
				array_push($arrayValues, $children);
			}
		}
		
		if($component !== "sw")
		{
			//Fem l'insert amb la taula corresponent.
			echo $db->insertComponent(MagicConstants::$DB_TABLES[$i], $arrayValues, $mac);
		}
	}
}

//S'encarrega d'afegir un usuari administrador si s'escau.
function addAdminIfNeeds($db)
{
	$id = uniqid();
	$userName = "admin";
	$mail = "admin@gmail.com";
	$pass = md5("patata");

	$select = "SELECT * FROM control_users WHERE email LIKE '%$email%';";

	if($db->checkIfExists($select) == 0) //Si no està, l'inserim.
		$db->executeSentence("INSERT INTO control_users VALUES('$id', '$userName', '$mail', '$pass', CURRENT_TIMESTAMP);");
}
#endregion FUNCTIONS

?>

