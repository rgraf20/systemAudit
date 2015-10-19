<?php

/**
	Fitxer que s'encarrega de les funcions d'apagar o encendre màquines.
*/

//S'encarrega d'apagar la màquina solicitada.
function shutdown($userName, $pass, $hostname)
{
	$exec = shell_exec("net rpc shutdown -U $userName%$pass -S $hostname -t 1");
	if (strpos($exec,'succeeded')) 
		return "$hostname computer shuts down correctly!";
	else
		return "Some error ocurred, host: $hostname";
}

//S'encarrega d'encendre la màquina solicitada.
function wol($mac, $broadcast)
{
    $mac_array = split(':', $mac);
    $hwaddr = '';

    //Agafem cada octet de la MAC.
    foreach($mac_array AS $octet)
    {
        $hwaddr .= chr(hexdec($octet));
    }

    //Creem el Magic Packet que rebrà la targeta de xarxa solicitada.
    $packet = '';
    for ($i = 1; $i <= 6; $i++)
    {
        $packet .= chr(255);
    }
    for ($i = 1; $i <= 16; $i++)
    {
        $packet .= $hwaddr;
    }

    //Creem el socket.
    $sock = socket_create(AF_INET, SOCK_DGRAM, SOL_UDP);
    if ($sock)
    {
        $options = socket_set_option($sock, 1, 6, true);

        if ($options >=0) 
        {    
            $e = socket_sendto($sock, $packet, strlen($packet), 0, $broadcast, 7); //L'enviem.
            socket_close($sock);
        }    
    }
    return "WOL sended: $mac";
}
?>
