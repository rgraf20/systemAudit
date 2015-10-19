/**
	Fitxer que conté les utilitats del detall d'un sistema.
*/

/*REGION VARIABLES_GLOBALS*/
DICT_FOR_SELECT = new Object();
DICT_FOR_SELECT["System"] = "systemId";
DICT_FOR_SELECT["Motherboard"] = "hwId";
DICT_FOR_SELECT["Bios"] = "hwId";
DICT_FOR_SELECT["Memory"] = "hwId";
DICT_FOR_SELECT["Video"] = "hwId";
DICT_FOR_SELECT["Processor"] = "hwId";
DICT_FOR_SELECT["Network"] = "hwId";
DICT_FOR_SELECT["Disk"] = "hwId";
DICT_FOR_SELECT["Windows"] = "windowsId";
DICT_FOR_SELECT["Installed"] = "installed";
/*ENDREGION VARIABLES_GLOBALS*/

/*REGION FUNCTIONS*/
//Funció AJAX que s'ecarrega d'agafar les dades de un sistema a travès de la mac.
function getSystemInfo()
{
	 var mac = sessionStorage.lastMac;
	 $.ajax({
	      type: "POST",
	      url: "../php/controllers/return_data.php",
	      data: {oneSystem: "", mac: mac},
	      success: function (result) {
	      	var json = undefined;
	      	try
	      	{
	      		json = JSON.parse(result);
	      		sessionStorage.setItem("lastSystem", result);
	      	}
	      	catch(ex)
	      	{
	      		console.log("Some error ocurred...");
	      	}
	      	if(json != undefined)
	      		createDiv("System");
	      }
	 });
}

//Funció AJAX que s'ecarrega de mantenir actualitzat l'estat del sistema actual ONLINE/OFFLINE.
function updateStatus()
{
	 var mac = sessionStorage.lastMac;
	 $.ajax({
	      type: "POST",
	      url: "../php/controllers/return_data.php",
	      data: {status: "", mac: mac},
	      success: function (result) {
			var lastSystem = getLastSystem();
	      	if(result.trim() == "ONLINE" || result.trim() == "OFFLINE") //Si és online o offline.
	      	{
	      		if(lastSystem["System"][0]["status"] != result.trim()) //Si és diferent del que ja tenia.
	      		{	
	      			lastSystem["System"][0]["status"] = result.trim(); //Modifiquem l'estat.
	      			sessionStorage.setItem("lastSystem", JSON.stringify(lastSystem)); //El tornem a guardar.

	      			//Si estem al sistema creem el seu div.
					var loc =  document.location.href.substring(document.location.href.indexOf("#") + 1, document.location.href.length);
					if(document.location.href.indexOf("#") == -1 || loc=="System")
						createDiv("System");
	      		}
	      	}
	      	console.log(result);
	      	
	      	//Tornem a fer la crida amb 5000 milisegons de retràs.
	      	setTimeout(updateStatus, 5000);
	      }
	 });
}

//S'encarrega de fer la crida ajax amb els paràmetres necessaris per apagar una màquina.
function shutdown()
{
	 var lastSystem = getLastSystem();
	 var userName = lastSystem["Windows"][0].registered_user;
	 var pass = lastSystem["Windows"][0].registered_pw;
	 var hostname = lastSystem["Windows"][0].hostname;

	 $.ajax({
	      type: "POST",
	      url: "../php/controllers/return_data.php",
	      data: {shut: "", userName: userName, pass: pass, hostname: hostname},
	      success: function (result) {
	      	if(result.indexOf("error") != -1) //Error.
	      		sweetAlert("Shutdown", result, "error");
	      	else
	      		swal("Shutdown", result + "\nWait for changes in status.", "success"); //Tot correcte.
	      }
	 });
}

//S'encarrega de fer la crida ajax amb els paràmetres necessaris per encendre una màquina.
function wol()
{
	 var lastSystem = getLastSystem();
	 var mac = lastSystem["System"][0].mac_address;
	 var broadcast = lastSystem["Network"][0].broadcasting;

	 $.ajax({
	      type: "POST",
	      url: "../php/controllers/return_data.php",
	      data: {wol: "", mac: mac, broad: broadcast},
	      success: function (result) {
	      	swal("WOL", result + " \nWait for changes in status.", "success");
	      }
	 });
}

//S'encarrega de retornar l'últim sistema guardat al sessionStorage.
function getLastSystem()
{
	return JSON.parse(sessionStorage.lastSystem);
}

//Funció que s'encarrega de assignar les propietats dels sistemes segons la secció als seus divs.
function createDiv(section)
{
	if(section == "Installed") //Crearem una taula diferent.
		{ createInstalledTable(); }
	else //Crearem el div estàndar.
	{
		var lastSystem = getLastSystem();
		var oneComponent = lastSystem[section][0];
		var imgPath = "../img/" + section + ".png"; 
		var stringFinal = '<div class="container">' +
							'<div class="row">' + 
								'<div class="col-sm-2">' + 
									'<img class="img-thumbnail imgMine" src="' + imgPath + '"/>' + 
								'</div>' + 
								'<div class="col-sm-10">' + 
									'<div class="list-group">' + 
										'<a class="list-group-item active centerMine"><b>' + section.toUpperCase() + '</b></a>';
		for (key in oneComponent) 
		{
			var value = oneComponent[key];

			//Comprovació per els labels.
			if(key == "status" && value.trim() == "OFFLINE") value = '<span class="label label-danger">' + value + "</span>"; 
			else if(key == "country") 
			{ var countryImg = "../img/" + value + ".png"; value = '<img class="img-thumbnail" src="' + countryImg + '" width="40" height="40"/>'; }
			else
			{
				if(value == "N/A") value = '<span class="label label-warning">' + value + "</span>";
				else
				{
					//Convertir unitats.
					if(section=="Memory")
					{
						if(key=="capacity")
							value = conversor(value, "GB", 3);
					}
					else if(section == "Video")
					{
						if(key=="memory")
							value = conversor(value, "GB", 3);
					}
					else if(section == "Disk")
					{
						if(key=="size" || key =="freespace")
							value = conversor(value, "GB", 3);
					}
					else if(section == "Processor")
					{
						if(key == "speed")
							value = conversor(value, "GH", 1);
					}
					value = '<span class="label label-success">' + value + "</span>";	
				}
				 
			}

			stringFinal += '<a class="list-group-item"><h4><span class="label label-default">' + key.toUpperCase() + '</span> ' + value + '</h4></a>';

		}
		stringFinal += "</div></div></div>";
		
		//Per afegir el botó per apagar o encendre el sistema.
		if(section == "System")
		{
			var status = lastSystem[section][0].status.trim();
			if(status == "ONLINE")
			{
				stringFinal += '<button id="off" onclick="wakeOrShut(this)" type="button" class="btn btn-lg btn-danger">' + 
      								'<span class="glyphicon glyphicon-stop"></span> Shutdown'
   								'</button></div>';
			}
			else if(status == "OFFLINE")
			{
				stringFinal += '<button id="on" onclick="wakeOrShut(this)" type="button" class="btn btn-lg btn-success">' + 
      								'<span class="glyphicon glyphicon-play-circle"></span> Wake on LAN'
   								'</button>';
			}
		}
		stringFinal += "</div>";

		$('#systemComponent').html(stringFinal);
	}
}

//S'encarrega de generar la taula de programes instal·lats del sistema.
function createInstalledTable()
{
	var lastSystem = getLastSystem();
	var oneComponent = lastSystem["sw"];
	var programs = oneComponent.length;

	var stringFinal = "<div class='container'>" + 
				"<div class='row'>" +
				"<div class='col-lg-12'>" +
				'<h1><button type="button" class="btn btn-primary btn-md"><b>Current Programs</b> <span class="badge"><b>' + programs + '</b></span></button></h1>' +
				"<table class='table table-striped table-hover'>" + 
    				"<thead>" + 
    					"<tr>" + 
        					'<th>Package Name</th>' + 
        					'<th>Version</th>' +
        					'<th>Vendor</th>' + 
        					'<th>Install location</th>' + 
        					'<th>URL</th>' + 
    					"</tr>" + 
    				"</thead>" +
			    	'<tbody>';

	//Per cada programa.
	for(var i=0; i<oneComponent.length; i++)
	{
		var oneProgram = oneComponent[i];
		stringFinal += "<tr>";
		for (key in oneProgram) //Per cada propietat del programa.
		{
			if(key != "id_serial" && key != "system_id" && key != "timestamp")
			{
				var value = oneProgram[key];

				//Comprovació per dades i labels.
				if(value == "N/A") value = '<span class="label label-warning">' + value + "</span>";
				if(key == "url")
				{
					if(value != "N/A")
						value = "<a href='" + value + "' target='#'>" + value + "</a>"; 
				}
				stringFinal += "<td>" + value + "</td>";
			}
		}
		stringFinal += "</tr>";

	}
	stringFinal += '</tbody></table></div></div></div></div>';

	$('#systemComponent').html(stringFinal);
}

//S'encarrega de fer la petició AJAX per encendre o apagar la màquina.
function wakeOrShut(obj)
{
	if(obj.id == "off")
		showConfirmShutdownAlert()
	else if(obj.id == "on")
		wol();
}

//S'encarrega de mostrar l'alert de confirmació de shutdown.
function showConfirmShutdownAlert()
{
	swal(
	{
		title: "Shutdown?",   
		text: "This action cannot be undone, you will receive a server response for your request status.",   
		type: "warning",   
		showCancelButton: true,   
		confirmButtonColor: "#DD6B55",   
		confirmButtonText: "Yes, i want",   
		cancelButtonText: "No, cancel!",   
		closeOnConfirm: false,   
		closeOnCancel: true 
	}, 
	function(isConfirm)
	{   
		if (isConfirm) //Si confirmen, apaguem el pc.
			shutdown();
	});
}

/*ENDREGION FUNCTIONS*/

/*REGION EVENTS*/
/*Degut a que no recarreguem la pàgina simularem una pila de webs amb aquest event, 
agafant la última part de la url i fent crida als meus mètodes com si fossin clicks.
Així aconseguim que quan clickem enrere fagi l'efecte com si la pàgina fos diferent.*/
window.onpopstate = function(event) 
{
	if(sessionStorage.user == undefined)
		document.location.href = "index.html";
	else
	{
		var loc =  document.location.href.substring(document.location.href.indexOf("#") + 1, document.location.href.length);
		if(document.location.href.indexOf("#") != -1)
			createDiv(loc);
		else
			createDiv("System");

	  	changeActiveItem(document.location.href.indexOf("#") != -1 ? DICT_FOR_SELECT[loc] : DICT_FOR_SELECT["System"]);
  	}
};

//MAIN:
/*Degut a la gestió explicada a l'event onpopstate hem de fer que la navegació del usuari sigui natural fent el comportament del reload nosaltres.*/
window.onload = function(event)
{
	if(sessionStorage.user == undefined)
		document.location.href = "index.html";
	else
	{
		var loc =  document.location.href.substring(document.location.href.indexOf("#") + 1, document.location.href.length);
		if(document.location.href.indexOf("#") != -1)
		{
			createDiv(loc);
			changeActiveItem(DICT_FOR_SELECT[loc]);
		}
		else
			getSystemInfo();

		//Fem la crida per l'status.
		updateStatus();
		$("#userName").html(JSON.parse(sessionStorage.user)["user"][0]["name"]);
	}
}
/*ENDREGION EVENTS*/
