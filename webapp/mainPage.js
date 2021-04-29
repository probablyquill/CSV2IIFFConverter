var uploadedFile;

function submitForm(event) {
    event.preventDefault();
    var formData = new FormData(document.querySelector('form'));
    var employee_name = formData.get('ename');
    var start_date = formData.get('sdate');
    var end_date = formData.get('edate');
    var csvfile = formData.get('csvfile');

    var stringList = employee_name + "|" + start_date + "|" + end_date + "|" + uploadedFile;
    console.log(stringList);
    var Http = new XMLHttpRequest();
    Http.open("POST", "convert_data", true);
    Http.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
    Http.send(stringList);

    Http.onreadystatechange = (e) => {
        if (Http.readyState == 4) {
            console.log("Recieved a response.");
            //console.log(Http.responseText);
            if (Http.responseText == "An error has occured." || Http.responseText == "") {
                console.log("An error has occured.");
                alert("An error has occured while converting the data. Please make sure that all of the date information was entered correctly.");
            } else if(Http.responseText == "Error1") {
                alert("An error has occured: Either the start or end date was entered incorrectly.");
            } else if(Http.responseText == "Error2") {
                alert("An error has occured: The end date is before the start date.");
            } else if(Http.responseText == "Error3") {
                alert("An error has occured: There was a problem parsing the CSV dates."); 
            }else {
                employee_name = employee_name.replace(",", "").replace(" ", "").toLowerCase();
                download(employee_name + start_date + "-" + end_date + ".iff", Http.responseText);
            }
            
        }
    }
}

function attatchJsToForm(formID) {
    document.getElementById(formID).addEventListener("submit", submitForm);
    testJS();
    const filegrabber = document.getElementById('csvupload');
    filegrabber.addEventListener('change', (event) => {
        var file = document.getElementById('csvupload').files[0];
        console.log(file);
        readFile(file);
    });
    
}

function testJS() {
    console.log("JS loaded correctly.");
}

function readFile(file) {
    var reader = new FileReader();
    reader.onload = (function(reader){
        return function() {
            uploadedFile = reader.result;
            console.log(uploadedFile);
        }
    })(reader);
    reader.readAsText(file);
}

function download(filename, text) {
    var element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
    element.setAttribute('download', filename);
    element.style.display = 'none';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
}