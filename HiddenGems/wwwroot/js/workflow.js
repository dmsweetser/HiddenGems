$(document).ready(function () {
    if (document.getElementById('sampleLink') !== null) {
        $('#sampleLink').bind('click', function (event) {
            event.preventDefault();
            event.stopImmediatePropagation();
            $('#onlineUpload').val('https://fiveloavestwofish.blob.core.windows.net/hiddengems/IsItOrIsntIt.csv');
            UploadData();
        });
    }

    if (document.getElementsByClassName('upload-link').length !== 0) {
        $('.upload-link').not('#sampleButton').bind('click', function (event) {
            $('#localFileUpload').val('');
            $('#onlineUpload').val('');
            $('.upload-link').val('upload-link-active');
            $('.upload-link').removeClass('upload-link-active');
            $(event.target).addClass('upload-link-active');
        });
    }

    if (document.getElementById('onlineUpload') !== null) {
        document.getElementById("onlineUpload").addEventListener("change", function () {
            document.getElementById("uploadNext").style.display = '';
        });
    }

    if (document.getElementById('onlineUpload') !== null) {
        document.getElementById("localFileUpload").addEventListener("change", function () {
            document.getElementById("uploadNext").style.display = '';
        });
    }

    if (sessionStorage.currentStep) {
        Navigate(null, sessionStorage.currentStep, sessionStorage.newStep);
    } else {
        Navigate(null, 'welcome', 'welcome');
    }

    window.hg.slideIntervalId = setInterval(function () {
        if (document.getElementById('welcome').style.display !== 'none') {
            showDivs(window.hg.slideIndex += 1);
        }
    }, 8000);
    showDivs(window.hg.slideIndex += 1);
});

function SwitchIntro(n) {
    clearInterval(window.hg.slideIntervalId);
    showDivs(window.hg.slideIndex += n);
}

//Inspired by https://www.w3schools.com/w3css/w3css_slideshow.asp
function showDivs(n) {
    var i;
    var x = document.getElementsByClassName("introItem");
    if (n > x.length) { window.hg.slideIndex = 1 }
    if (n < 1) { window.hg.slideIndex = x.length }
    for (i = 0; i < x.length; i++) {
        x[i].style.display = "none";
    }
    x[window.hg.slideIndex - 1].style.display = "block";
}

function Activate() {
    var formToSubmit = new FormData();
    formToSubmit.append('activationKey', document.getElementById('activationCode').value);
    $.ajax({
        url: 'api/Job/Activate',
        type: 'POST',
        data: formToSubmit,
        processData: false,
        contentType: false,
        success: function (result) {
            if (result === true) {
                sessionStorage.removeItem('currentStep');
                sessionStorage.removeItem('newStep');
                location.reload(true);
            } else {
                sessionStorage.errorMessage = 'License could not be activated. Please ensure you have a working internet connection '
                    + 'and that your license has not already been used. '
                    + 'If you are still having issues, please email support@hiddengemsml.com.';
                Navigate(null, 'activate', 'error');
            }
        },
        error: function (error) {
            sessionStorage.errorMessage = JSON.stringify(error);
            Navigate(null, 'activate', 'error');
        }
    });
}

function ResendEmail() {
    var formToSubmit = new FormData();
    formToSubmit.append('emailAddress', document.getElementById('emailAddress').value);
    $.ajax({
        url: 'api/Job/ResendEmail',
        type: 'POST',
        data: formToSubmit,
        processData: false,
        contentType: false,
        success: function (result) {
            if (result) {
                document.getElementById('submitEmailMessage').style.display = '';
            } else {
                sessionStorage.errorMessage = 'Activation Code resend was unsuccessful. '
                    + 'Please make sure you have entered the email address that you used when you purchased Hidden Gems Local Edition. '
                    + 'If you are still having issues, feel free to email support@hiddengemsml.com.';
                Navigate(null, 'activate', 'error');
            }
        },
        error: function (error) {
            sessionStorage.errorMessage = JSON.stringify(error);
            Navigate(null, 'activate', 'error');
        }
    });
}

function Navigate(event, currentStep, newStep) {
    if (event) {
        event.preventDefault();
        event.stopImmediatePropagation();
    };

    document.getElementById(currentStep).classList.add('animate__animated');
    document.getElementById(currentStep).classList.add('animate__fadeOutLeft');
    void document.getElementById(newStep).offsetWidth;
    PreInitNewStep(currentStep, newStep);

    setTimeout(function () {
        //Hides the current step and its buttons
        document.getElementById(currentStep).style.display = 'none';
        document.querySelector('div .' + currentStep).style.display = 'none';
        document.getElementById(newStep).classList.remove('animate__animated');
        document.getElementById(newStep).classList.remove('animate__fadeOutLeft');
        //Shows the new step
        document.getElementById(newStep).classList.add('animate__animated');
        document.getElementById(newStep).classList.add('animate__fadeInRight');
        document.getElementById(newStep).style.display = '';
        document.querySelector('div .' + newStep).style.display = '';
        sessionStorage.currentStep = currentStep;
        sessionStorage.newStep = newStep;

        if (newStep === 'forecast') {
            setTimeout(Forecast(), 2000);
        }

    }, 500);
}

function PreInitNewStep(currentStep, newStep) {
    switch (newStep) {
        case 'welcome':
            sessionStorage.removeItem('availableColumns');
            sessionStorage.removeItem('selectedColumn');
            sessionStorage.removeItem('analysisResults');
            sessionStorage.errorMessage = '';
            break;
        case 'upload':
            sessionStorage.removeItem('availableColumns');
            sessionStorage.removeItem('selectedColumn');
            sessionStorage.removeItem('analysisResults');
            sessionStorage.errorMessage = '';
            window.hg.jobWasCancelled = false;
            $('#localFileButton').click();
            break;
        case 'pick':
            sessionStorage.removeItem('selectedColumn');
            sessionStorage.removeItem('analysisResults');
            sessionStorage.errorMessage = '';
            if (sessionStorage.availableColumns
                && document.getElementsByClassName('pickButton').length === 0) {
                var availableColumns = JSON.parse(sessionStorage.availableColumns);
                PopulatePicklist(availableColumns);
            }
            break;
        case 'check':
            sessionStorage.removeItem('analysisResults');
            document.getElementById("pickNext").style.display = 'none';
            break;
        case 'result':
        case 'forecast':
            sessionStorage.removeItem('errorMessage');
            if (sessionStorage.analysisResults
                && document.getElementsByClassName('individualResult').length === 0) {
                var results = JSON.parse(sessionStorage.analysisResults);
                PopulateResults(results);
            }
            break;
        case 'error':
            if (sessionStorage.errorMessage
                && sessionStorage.errorMessage !== ''
                && sessionStorage.errorMessage !== undefined) {
                document.getElementById('error-text').innerText = sessionStorage.errorMessage;
            } else {
                sessionStorage.errorMessage = 'Something interesting happened, but we can\'t quite say what!';
                document.getElementById('error-text').innerText = sessionStorage.errorMessage;
            }
            break;
        default:
    }
}

function UploadData() {
    var formToSubmit = new FormData();

    sessionStorage.removeItem('availableColumns');
    $('#pickButtonGroup .pickButton').remove();
    var buttonsArray = document.querySelectorAll('.pickButton');
    for (var i = 0; i < buttonsArray.length; i++) {
        buttonsArray[i].parentNode.removeChild(buttonsArray[i])
    }
    var fileToUpload = $('#localFileUpload').prop('files')[0];

    var onlineUpload = $('#onlineUpload').val();
    if (fileToUpload !== undefined && fileToUpload !== null && fileToUpload.size === 0 && onlineUpload.length === 0) return;
    var uploadUrl = "";
    if (fileToUpload !== undefined && fileToUpload !== null && fileToUpload.size > 0) {
        uploadUrl = 'api/Job/UploadDataFile';
        formToSubmit.append('dataToUpload', fileToUpload);
    } else if (onlineUpload.length > 0) {
        uploadUrl = 'api/Job/UploadDataUrl';
        formToSubmit.append('dataToUpload', onlineUpload);
    }

    $.ajax({
        url: uploadUrl,
        type: 'POST',
        data: formToSubmit,
        processData: false,
        contentType: false,
        success: function (result) {
            if (result !== null && result.length > 0) {
                sessionStorage.availableColumns = JSON.stringify(result);
                PopulatePicklist(result);
            } else {
                Navigate(null, 'check', 'error');
            }
        },
        error: function (error) {
            sessionStorage.errorMessage = JSON.stringify(error);
            Navigate(null, 'check', 'error');
        }
    });
    CheckStatus('upload', true);
}

function PopulatePicklist(results) {
    for (var i = 0; i < results.length; i++) {
        if (results[i].item2 === '') {
            results[i].item2 = results[i].item1;
        }

        document.getElementById('pickButtonGroup').innerHTML +=
            '<button type="button" class="btn pickButton"'
            + ' name="' + results[i].item2 + '"'
            + ' onclick = "SpecifyColumnToAnalyze(\''
            + results[i].item1 + '\')">' + results[i].item2 + '</button>';
    }

    if (document.getElementsByName('ShouldIBuyTheCar')[0] !== undefined) {
        var allPickButtons = document.getElementsByClassName('pickButton');
        var pickButtonsArray = Array.from(allPickButtons);
        for (var i = 0; i < pickButtonsArray.length; i++) {
            pickButtonsArray[i].style.display = 'none';
        }
        document.getElementsByName('ShouldIBuyTheCar')[0].style.display = '';
    }
}

function CheckStatus(previousStep, isUpload) {
    if (window.hg.jobWasCancelled || sessionStorage.errorMessage !== '') return;
    Navigate(null, previousStep, 'check');
    if (isUpload) {
        $('.check .previousButton').hide();
    } else {
        $('.check .previousButton').show();
    }
    $.ajax({
        url: 'api/Job/CheckStatus',
        type: 'GET',
        success: function (result) {
            var statusElement = document.getElementById('currentStatus');
            if (statusElement && result.statusMessage !== null && result.statusMessage.length > 0) statusElement.innerText = result.statusMessage;
            if (isUpload) {
                $('#progressBar')
                    .attr('aria-valuenow', result.uploadPercentCompleted * 100)
                    .css('width', result.uploadPercentCompleted * 100 + '%');
            } else {
                $('#progressBar')
                    .attr('aria-valuenow', result.percentCompleted * 100)
                    .css('width', result.percentCompleted * 100 + '%');
            }
            if (isUpload && result.uploadCompleted === true) {
                setTimeout(function () {
                    Navigate(null, 'check', 'pick');
                }, 2000);
            } else if (result.jobCompleted === true) {
                GetResult();
            } else if (result.errorMessage !== undefined && result.errorMessage !== '') {
                sessionStorage.errorMessage = result.errorMessage;
                Navigate(null, 'check', 'error');
            } else if (sessionStorage.errorMessage
                && sessionStorage.errorMessage !== ''
                && sessionStorage.errorMessage !== undefined) {
                Navigate(null, 'check', 'error');
            } else {
                setTimeout(function () {
                    CheckStatus(previousStep, isUpload);
                }, 2000);
            }
        },
        error: function (error) {
            sessionStorage.errorMessage = JSON.stringify(error);
            Navigate(null, 'check', 'error');
        }
    });
}

function ResetJob(previousStep) {
    $.ajax({
        url: 'api/Job/Cancel',
        type: 'DELETE',
        success: function (result) {
            window.hg.jobWasCancelled = true;
            Navigate(null, previousStep, 'welcome');
        }
    });
}

function SpecifyColumnToAnalyze(selectedColumn) {
    document.getElementById("pickNext").style.display = '';
    sessionStorage.selectedColumn = selectedColumn;
    $('.pickButton').removeClass('selectedColumn');
    $(event.target).addClass('selectedColumn');
}

function BeginAnalysis() {
    var selectedColumns = [];
    selectedColumns.push(sessionStorage.selectedColumn);
    var formToSubmit = new FormData();
    if (selectedColumns.length > 0) {
        formToSubmit.append('selectedColumns', selectedColumns);
        $.ajax({
            url: 'api/Job/SubmitRequest',
            type: 'POST',
            data: formToSubmit,
            processData: false,
            contentType: false,
            success: function (result) {
                CheckStatus('pick', false);
            },
            error: function (error) {
                sessionStorage.errorMessage = JSON.stringify(error);
                Navigate(null, 'check', 'error');
            }
        });
    }
}

function GetResult() {
    $.ajax({
        url: 'api/Job/GetResult',
        type: 'GET',
        processData: false,
        contentType: false,
        success: function (results) {
            sessionStorage.analysisResults = JSON.stringify(results);
            PopulateResults(results);
            setTimeout(function () {
                Navigate(null, 'check', 'result');
            }, 2000);
        },
        error: function (error) {
            sessionStorage.errorMessage = JSON.stringify(error);
            Navigate(null, 'check', 'error');
        }
    });
}

function PopulateResults(results) {
    var selectedColumnName = JSON.parse(sessionStorage.availableColumns)
        .filter(function (x) { return x.item1 === sessionStorage.selectedColumn; })[0].item2;
    if (selectedColumnName) {

        document.getElementsByClassName('result-helper')[0].innerText = 'What column contributed most to ' + selectedColumnName + '?';

        if (results.columnWeights === null
            || results.columnWeights === undefined
            || JSON.stringify(results.columnWeights) === JSON.stringify({})) {
            document.getElementById('resultCollection').innerHTML =
                "<div class='individualResult'><span style='font-size: 1.5rem; opacity: 1'>"
                + "No valid results found</span></div>";
            document.getElementById('rawResult').innerHTML =
                "<div class='individualResult'><span style='font-size: 1.5rem; opacity: 1'>"
                + "No valid results found</span></div>";
        }
        else {
            var constructedResult = '';

            var columnsInSignificanceOrder = [];

            for (var i = 0; i < results.columnWeights.length; i++) {
                var individualWeight = results.columnWeights[i];
                individualWeight.value = (Number(individualWeight.value) * 1.2).toString();
                constructedResult +=
                    "<div class='individualResult'><span style='font-size: "
                    + individualWeight.value + "rem; opacity: "
                    + individualWeight.value + "'>"
                    + individualWeight.key + "</span></div>";
                if (individualWeight.value !== '0') {
                    columnsInSignificanceOrder.push(individualWeight.key);
                }
            }

            document.getElementById('resultCollection').innerHTML = constructedResult;
            document.getElementById('rawResult').innerHTML = results.rawResult;

            if (results.serializedSampleRecord) {
                PopulateForecasting(results.serializedSampleRecord, selectedColumnName, columnsInSignificanceOrder);
            }
        }
    }
}

function PopulateForecasting(serializedSampleRecord, selectedColumnName, columnsInSignificanceOrder) {
    document.getElementById('resultNext').style.display = '';
    document.getElementsByClassName('forecast-tag')[0].innerHTML = 'Change the values below to see the estimated value for <strong>'
        + selectedColumnName + '</strong>:';

    var parsedSampleRecord = JSON.parse(serializedSampleRecord);
    var constructedResult = '';

    var individualInputsToAdd = [];

    for (let keyProp in parsedSampleRecord) {
        var encodedKeyProp = encodeURI(keyProp);
        var relativeSignificance = columnsInSignificanceOrder.indexOf(keyProp);
        if (keyProp === selectedColumnName || relativeSignificance === -1) continue;
        individualInputsToAdd[relativeSignificance] = "<div class='individualInput'>"
            + "<label for=\"" + encodedKeyProp + "\" class=\"individualInputLabel\">" + keyProp + "</label>"
            + "<input type=\"text\" id=\"" + encodedKeyProp + "\" value=\"" + parsedSampleRecord[keyProp] + "\" class=\"form-control individualInputField\">"
            + "</div>";
    }

    for (var i = 0; i < individualInputsToAdd.length; i++) {
        constructedResult += individualInputsToAdd[i];
    }

    document.getElementById('forecast-inputs').innerHTML = constructedResult;
    document.getElementById('forecast-output').innerHTML = "<hr/><div class='output'>"
        + "<label for=\"outputField\" class=\"outputLabel\">" + selectedColumnName + "</label>"
        + "<input type=\"text\" id=\"outputField\" class=\"form-control outputField\" readonly>"
        + "</div>";

    var allForecastInputs = document.getElementsByClassName('individualInputField');

    for (var i = 0; i < allForecastInputs.length; i++) {
        allForecastInputs[i].addEventListener('keyup', function (event) {
            Forecast();
        });
    }
}

function OpenDetailedResult() {
    var newWindow = window.open();
    newWindow.document.write("<!DOCTYPE html><html style=\"overflow:auto;\"><head><meta charset=\"utf-8\" /><title>Hidden Gems Detailed Result</title><link rel=\"stylesheet\" href=\"./lib/bootstrap/dist/css/bootstrap.min.css\" /><link rel=\"stylesheet\" href=\"./css/site_20210915.css\" /><link rel=\"icon\" type=\"image/png\" href=\"./favicon.ico\"></head><body style=\"overflow:auto; height:100vh; font-size:1.5rem;\"><div id=\"rawResult\" class=\"workflow-step container\" style=\"height:100vh;\">"
        + document.getElementById('rawResult').innerHTML
        + "</div ></body ></html > ");
    newWindow.document.close();
}

var forecastTimeout;

function Forecast() {

    document.getElementById('outputField').value = '';

    if (forecastTimeout !== undefined) {
        clearTimeout(forecastTimeout);
    }

    foreCastTimeout = setTimeout(function () {
        document.getElementById('outputField').classList.remove('get-attention');

        var parsedResult = JSON.parse(sessionStorage.analysisResults);
        var parsedSampleRecord = JSON.parse(parsedResult.serializedSampleRecord);

        var availableInputs = document.getElementsByClassName('individualInputField');
        for (var i = 0; i < availableInputs.length; i++) {
            parsedSampleRecord[decodeURI(availableInputs[i].id)] = availableInputs[i].value;
        }

        var formToSubmit = new FormData();
        formToSubmit.append('recordToAnalyze', JSON.stringify(parsedSampleRecord));

        $.ajax({
            url: 'api/Job/Evaluate',
            type: 'POST',
            data: formToSubmit,
            processData: false,
            contentType: false,
            success: function (result) {
                if (result.item1 && result.item1 !== '') {
                    document.getElementById('outputField').value = result.item1 + '(' + result.item2.replace(' %', '%') + ' certain)';
                } else {
                    document.getElementById('outputField').value = result.item2;
                }
                document.getElementById('outputField').classList.add('get-attention');
            },
            error: function (error) {
                sessionStorage.errorMessage = JSON.stringify(error);
                Navigate(null, 'forecast', 'error');
            }
        });
    }, 1500);
}

function Buy() {
    window.open('https://buy.stripe.com/8wM16Pca6aBDd1e8wx', '_blank');
}


