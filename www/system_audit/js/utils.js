/**
	Fitxer que conté funcions d'utilitats.
*/

/*REGION VARIABLES_GLOBALS*/
REGEX_MAIL= /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
/*ENDREGION VARIABLES_GLOBALS*/

/*S'encarrega de redireccionar a la pàgina pasada.*/
function hrefTo(str)
{
	window.location.href = str + ".html";
}

/*S'encarrega de canviar de item actiu al menú, ja que per defecte bootstrap no ho fa.*/
function changeActiveItem(id)
{
	$(".menuItemMine").removeClass("active");
	$("#" + id).addClass("active");
}

//Comprova les dades del login i el login.
function checkLogin()
{
	var mail = document.getElementById("mail").value;
	var pass = document.getElementById("pass").value;

	if(mail=="" || pass=="")
		sweetAlert("Login", "Mail and pass cannot be empty.", "error");
	else
	{
		if(!mail.match(REGEX_MAIL)) //Si no passa l'expressió regular.
			sweetAlert("Login", "Put a valid mail address.", "error");
		else
		{
			$.ajax({
			  type: "POST",
			  url: "../php/controllers/return_data.php",
			  data: {checkLogin: "", mail: mail, pass: pass},
			  success: function (result) {
			  	try
			  	{
			  		var user = JSON.parse(result);
			  		sessionStorage.setItem("user", result); //Si podem parsejar el resultat voldrar dir que podem entrar, si es així guardem el resultat a sessionStorage perquè sigui més fàcil d'accedir a ell.
			  		document.location.href = "systems.html";
			  	}
			  	catch(ex) //Sinó alertem amb el missatge.
			  	{
					sweetAlert("Login", result, "error");
			  	}
			  }
			});
		}
	}
}

//Fa logout eliminant la key del sessionStorage.
function logout()
{
	$.ajax({
	  type: "POST",
	  url: "../php/controllers/return_data.php",
	  data: {logout: ""},
	  success: function (result) {
	  	sessionStorage.removeItem("user");
		window.location.href="index.html";
	  }
	});
}

//S'encarrega d'afegir un usuari a la BD.
function addUser()
{
	var userName = document.getElementById("userNameAdd").value;
	var mail = document.getElementById("userMailAdd").value;
	var pass = document.getElementById("userPwdAdd").value;

	if(userName == "" || mail == "" || pass == "")
		sweetAlert("Add user", "Name, Mail and pass cannot be empty.", "error");
	else
	{
		if(!mail.match(REGEX_MAIL))
			sweetAlert("Add user", "Put a valid mail address.", "error");
		else
		{
			$.ajax({
			  type: "POST",
			  url: "../php/controllers/return_data.php",
			  data: {addUser: "", userName: userName, mail: mail, pass: pass},
			  success: function (result) {
			  	console.log(result);
			  	if(result.trim() == "1") //Correcte.
			  		swal("Add User", "User Add succesfully!", "success");
			  	else //Error.
			  		swal("Add User", "Mail already registered.", "error");
			  }
			});
		}
	}
}

//S'encarrega de convertir una unitats en unes altres depenent d'elles i dels passos especificats.
function conversor(toConvert, unity, steps)
{
	var result = toConvert;
	for(var i=0;i<steps;i++)
		result/=unity == "GH" ? 1000 : 1024;
	return result.toFixed(2) + " " + unity;
}

//MAIN:
//Per activar els tooltips.
$(document).ready(function(){
    $('[data-toggle="tooltip"]').tooltip(); 
});