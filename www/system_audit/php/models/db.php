<?php

/**
	Classe que ens centralitza les connexions a la BD i també serveix per retornar dades.
*/

#region HEADERS
header('Content-Type: text/html; charset=utf-8');
#endregion HEADERS

#region INCLUDES
include_once('/var/www/system_audit/php/utils/magic_constants.php');
#endregion INCLUDES

class DB
{
	//Atributs:
	var $con;

	//Constructor:
	function __construct()
	{
		$this->connect();
	}

	#region FUNCTIONS
	//Connectar:
	function connect()
	{
		$this->con=mysqli_connect(MagicConstants::$DB_CONFIG["url"], MagicConstants::$DB_CONFIG["user"],
					 MagicConstants::$DB_CONFIG["pass"], MagicConstants::$DB_CONFIG["schema"]);
		if (mysqli_connect_errno())
			echo "Errada: " . mysqli_connect_error();
	}

	//Tancar:
	public function close()
	{
		mysqli_close($this->con);
	}
	
	//S'encarrega d'executar una sentència.
	public function executeSentence($sentence)
	{
		$result = mysqli_query($this->con, $sentence);
		if(mysqli_error($this->con)) //Error.
			$toEcho = "Error amb la sentència:\r\n" .$sentence . "\r\n" . mysqli_error($this->con) . "\r\n\r\n";
		else //Correcte.
			$toEcho = "Tot ok, sentència: \r\n " . $sentence . "\r\n";
		
		return $toEcho;
	}
	
	//S'encarrega d'insertar un component a travès del nom de la taula i l'array de valors.
	public function insertComponent($tableName, $valuesArray, $mac = '')
	{
		//Si la taula no és sistem afegim la mac al array de valors.
		if($tableName !== "system") { array_splice($valuesArray, 1, 0, array($mac)); }
		
		//Generem l'insert.
		$insert = "INSERT INTO " . $tableName . " VALUES (";
		for($i = 0; $i < sizeof($valuesArray); $i++)
		{
			$value = $this->con->real_escape_string(utf8_decode($valuesArray[$i]));
			if($i == 0) $value = $this->checkId($value);
			$insert .= "'$value', ";
		}			
		$insert .= "CURRENT_TIMESTAMP);";
		
		//Execució.
		return $this->executeSentence($insert);
	}
	
	//Determina si hi han registres de segons quina select.
	public function checkifExists($select)
	{
		$result = mysqli_query($this->con, $select);
		return mysqli_num_rows($result);
	}

	//Determina si un sistema existeix fent selects a partir de la MAC.
	public function checkIfSystemExists($mac)
	{
		return $this->checkifExists("SELECT * FROM system WHERE mac_address LIKE '%$mac%';");
	}

	//S'encarrega de retornar totes les ip's i macs del sistema.
	public function returnAllIps()
	{
		$result = mysqli_query($this->con, "SELECT sys.mac_address, ned.ip FROM system sys, network_details ned WHERE sys.mac_address = ned.system_id ORDER BY sys.status DESC, sys.timestamp DESC;");
		return $result;
	}
	
	//S'encarrega d'eliminar un sistema fent la crida a executeSentence amb la MAC.
	public function deleteSystem($mac)
	{
		return $this->executeSentence("DELETE FROM system WHERE mac_address LIKE '%$mac%';");
	}
	
	//S'encarrega de generar un id únic si aquest no s'ha pogut aconseguir.
	public function checkId($id)
	{
		if($id == "N/A")
			return uniqid();
		return $id;
	}
	
	//S'encarrega de retornar tots els registres de una taula donada mapejats per tal de fer el JSON encode posteriorment.
	public function getJSON($key, $tableName, $mac, $output)
	{

		if($key == "System")
			$sentence = "SELECT * FROM $tableName WHERE mac_address LIKE '%$mac%';";
		else if($key == "sw")
			$sentence = "SELECT * FROM $tableName WHERE system_id LIKE '%$mac%' ORDER BY package_name;";
		else if($key == "user")
			$sentence = "SELECT * FROM control_users WHERE email LIKE '%$mac%';";
		else
			$sentence = "SELECT * FROM $tableName WHERE system_id LIKE '%$mac%';";

		$result = mysqli_query($this->con, $sentence);
		while($row = mysqli_fetch_assoc($result)) 
		{
  			$output["$key"][]=array_map("utf8_encode", $row);
		}
		return $output;
	}

	//S'encarrega d'enviar tots els sistemes en forma de JSON.
	public function getAllSystems()
	{
		$sentence = "SELECT sys.mac_address, nd.ip, wd.hostname, wd.country, sys.status
				FROM system sys, network_details nd, windows_details wd
				WHERE sys.mac_address = nd.system_id AND sys.mac_address = wd.system_id ORDER BY sys.status DESC, sys.timestamp DESC;";

		$result = mysqli_query($this->con, $sentence);

		while($row = mysqli_fetch_assoc($result)) 
		{
  			$output["allSystems"][]=array_map("utf8_encode", $row);
		}

		return json_encode($output);
	}

	/*S'encarrega de retornar un sistema a través de la seva mac, cridant al mètode (getJSON) amb un paràmetre que actúa com a referència,
	ja que serà el que retornarem desprès d'haver fet totes les crides*/
	public function getOneSystem($mac)
	{
		$output = $this->getJSON("System", "system", $mac, array());
		$output = $this->getJSON("Motherboard", "motherboard_details", $mac, $output);
		$output = $this->getJSON("Bios", "bios_details", $mac, $output);
		$output = $this->getJSON("Memory", "memory_details", $mac, $output);
		$output = $this->getJSON("Video", "video_details", $mac, $output);
		$output = $this->getJSON("Processor", "processor_details", $mac, $output);
		$output = $this->getJSON("Network", "network_details", $mac, $output);
		$output = $this->getJSON("Disk", "disk_details", $mac, $output);
		$output = $this->getJSON("Windows", "windows_details", $mac, $output);
		$output = $this->getJSON("sw", "installed_sw", $mac, $output);
		return json_encode($output);
	}

	//S'encarrega d'agafar tots els status i enviar-los en forma de json a travès de un string codificat amb | per separar-lo.
	public function getStatusAll($macs)
	{
		$macsArray = explode('|', $macs); //Array de macs.
		$statuses;
		for($i=0;$i<sizeof($macsArray);$i++)
		{
			$mac = $macsArray[$i];
			$status = $this -> getStatus($mac);
			$statuses[$mac] = $status;
		}
		return json_encode($statuses);
	}

	//S'encarrega de retornar l'status de una màquina donada.
	public function getStatus($mac)
	{
		$sentence = "SELECT status FROM system WHERE mac_address LIKE '%$mac%';";
		$result = mysqli_query($this->con, $sentence);

		$row = $result->fetch_row();
		$status = $row[0];
		$status = trim($status, "\n");
		$status = trim($status, "\t");
		$status = str_replace(' ', '', $status);
		return $status;
	}

	//S'encarrega d'afegir un usuari a la BD.
	function addUser($userName, $mail, $pass)
	{
		$id = uniqid();
		$pass = mysqli_real_escape_string($this->con, utf8_decode($pass));
		$pass = md5($pass);
		$userName = mysqli_real_escape_string($this->con, utf8_decode($userName));
		$mail = mysqli_real_escape_string($this->con, utf8_decode($mail));
		$exists = $this->checkifExists("SELECT * FROM control_users WHERE email='$mail';");
		if($exists != 0)
			return "0"; //Error.
		else
		{
			$this->executeSentence("INSERT INTO control_users VALUES('$id', '$userName', '$mail', '$pass', CURRENT_TIMESTAMP);");	
			return "1"; //Correcte.
		}
	}
	#region LOGIN FUNCTIONS
	//S'encarrega de fer el check del login.
	public function checkLogin($mail, $pass)
	{
		$dummy = array();
		$mail = mysqli_real_escape_string($this->con, utf8_decode($mail));
		$pass = mysqli_real_escape_string($this->con, utf8_decode($pass));
		$pass = md5($pass);
		$exists = $this->checkifExists("SELECT * FROM control_users WHERE email='$mail' AND password='$pass';");
		if($exists == 0)
			return "Invalid credentials...";
		else 
		{
			$_SESSION["logged"] = true; //Guardem el login.
			return json_encode($this->getJSON('user', 'control_users', $mail, $dummy));
		}
	}
	#endregion LOGIN_FUNCTIONS
	#endregion FUNCTIONS
}
?>
