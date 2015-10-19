<?php

/**
	Classe que conté com a variables estàtiques, constants indispensables per l'aplicatiu.
*/

class MagicConstants
{
	#region STATIC_VARIABLES
	public static $COMPONENTS_ARRAY;
	public static $DB_CONFIG;
	public static $DB_TABLES;
	#endregion STATIC_VARIABLES

	//Inicialitza totes les variables estàtiques.
	static function init()
	{
		self::$COMPONENTS_ARRAY = array("system", "motherboard", "bios", "memory", "video", "processor", "network", "disk", "windows", "sw");
		self::$DB_CONFIG = array("url" => "localhost", "user" => "root", "pass" => "patata", "schema" => "system_audit");
		self::$DB_TABLES = array("system", "motherboard_details", "bios_details", "memory_details", "video_details", "processor_details", "network_details", "disk_details", "windows_details");
	}
}
//Crida al init.
MagicConstants::init();

?>
