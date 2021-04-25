var uploadedFile;

function submitForm(event) {
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