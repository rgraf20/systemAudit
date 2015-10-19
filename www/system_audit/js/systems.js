/**
	Fitxer que conté les utilitats dels sistemes.
*/

/*REGION VARIABLES_GLOBALS*/
ARRAY_MACS_FOR_STATUS = new Object();
COLS_SYSTEM = 4;
/*ENDREGION VARIABLES_GLOBALS*/

/*REGION FUNCTIONS*/
//Funció AJAX que s'ecarrega d'agafar tots els sistemes.
function getAllSystems()
{
	if(sessionStorage.user == undefined)
		document.location.href = "index.html";
	else
	{
		$.ajax({
		  type: "POST",
		  url: "../php/controllers/return_data.php",
		  data: {allSystems: ""},
		  success: function (result) {
		  	gestionResponse(result);
		  	updateAllStatus();
		  }
		});
	}
}

//Funció AJAX que s'ecarrega de mantenir actualitzats l'estat de tots els sistemes.
function updateAllStatus()
{
	 $.ajax({
	      type: "POST",
	      url: "../php/controllers/return_data.php",
	      data: {statusAll: "", macs: getMacsStringForAJAX()},
	      success: function (result) {
	      	console.log(result);
	      	var macs = JSON.parse(result); //Agafem les macs.
	      	gestionUpdateAllStatus(macs);
	      	
	      	//Tornem a fer la crida amb 5000 milisegons de retràs.
	      	setTimeout(updateAllStatus, 5000);
	      }
	 });
}

//S'encarrega de gestionar l'estat dels sistemes.
function gestionUpdateAllStatus(jsonObj)
{
	for(key in ARRAY_MACS_FOR_STATUS) //Per cada mac.
	{
		if(ARRAY_MACS_FOR_STATUS[key] != jsonObj[key].trim()) //Si són diferents.
		{
			ARRAY_MACS_FOR_STATUS[key] = jsonObj[key].trim(); //Fem l'update del valor.
			document.getElementById(key).src = "../img/" + ARRAY_MACS_FOR_STATUS[key] + ".png"; //Fem l'update de la imatge.
		}
	}
}

//S'encarrega de generar l'string de les macs per la connexió ajax.
function getMacsStringForAJAX()
{
	var toReturn = "";
	for(key in ARRAY_MACS_FOR_STATUS)
		toReturn += key + "|"; //Per cada mac afegim el seu string i un | per desprès a php convertir-lo a un array.
	return toReturn.substring(0, toReturn.length -1);
}

//Funció que s'encarrega de assignar els sistemes als seus divs.
function gestionResponse(result)
{
	var stringGeneral = '<div class="container">';
	
	var json = JSON.parse(result);
	var system;
	var counterForRow = 0;

	$("#countSystems").html(" " + json.allSystems.length); //Assignem el counter dels sistemes que tenim a la xarxa.

	var i = 0;
	while(i < json.allSystems.length) //Per cada sistema.
	{
		stringGeneral += "<div class='row'>";
		for(var j=0;j<COLS_SYSTEM;j++) //Fem 4 columnes.
		{
			system = json.allSystems[i];
			var mac = system.mac_address;
			ARRAY_MACS_FOR_STATUS[mac] = system.status.trim();
			var statusImg = "../img/" + system.status.trim() + ".png";
			var macToHtml = mac == "N/A" ? ('<span class="label label-warning">' + mac + "</span>") : ('<span class="label label-success">' + mac + "</span>");
			var ip = system.ip;
			ip = ip == "N/A" ? ('<span class="label label-warning">' + ip + "</span>") : ('<span class="label label-success">' + ip + "</span>");
			var hostname = system.hostname;
			hostname = hostname == "N/A" ? ('<span class="label label-warning">' + hostname + "</span>") : ('<span class="label label-success">' + hostname + "</span>");
			var countryImg = "../img/" + system.country + ".png";

			stringGeneral += '<div class="col-sm-3">' + //Farem que cada sistema ocupi una quarta part del container.
								"<img onclick=\"clickSystem('" + mac + "')\" class='img-thumbnail pointerMine' src='../img/computer_logo.png'/>" + 
									'<div class="list-group">' + 
										'<a class="list-group-item"><h4><span class="label label-primary">Mac</span> ' + macToHtml + '</h4></a>' + 
										'<a class="list-group-item"><h4><span class="label label-default">Ip</span> ' + ip + '</h4></a>' +
										'<a class="list-group-item"><h4><span class="label label-default">Hostname</span> ' + hostname + '</h4></a>' +
										'<img class="img-thumbnail" src="' + countryImg + '" width="40" height="40"/> ' +
										'<img id="' + mac + '"class="img img-thumbnail" src="' + statusImg + '" width="40" height="40"/> ' +
									'</div>' +
							 '</div>';
			i++;
			if(i == json.allSystems.length) break; //Si la i ha arribat al length del array la parem.
		}
		stringGeneral += "</div>";
	}

	stringGeneral += '</div>'; //La resta de sistemes s'autoadjustarà.
	$("#containerSystems").html(stringGeneral); //Assignem el contingut al container de sistemes.
	$("#userName").html(JSON.parse(sessionStorage.user)["user"][0]["name"]);
}

//S'encarrega de gestionar el click de un sistema guardant l'últim sistema al qual li hem fet click a localstorage per obtenir-la posteriorment a details.
function clickSystem(mac)
{
	sessionStorage.setItem('lastMac', mac);
	hrefTo("detail");
}
/*ENDREGION FUNCTIONS*/

//MAIN:
$(document).ready(getAllSystems);
