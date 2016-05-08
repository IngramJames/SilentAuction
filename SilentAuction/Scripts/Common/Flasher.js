var flashElements = [];
var flashStepsBack = [];
var flashStepsFore = [];
var flasher = null;


// get the step size required to get between two colors in a given number of steps
function getStepSize(from, to, noOfSteps) {
    if (from == to) {
        return 0;   // the only valid case for zero
    }
    var difference = to - from;
    var stepSize = difference / noOfSteps;
    return stepSize;
}

// get the next step color
function getStepColor(currentColor, delta, totalSteps, thisStep, finalColor) {
    var newColor = currentColor + delta;

    // do not go past the target color
    if(delta < 0){
        if (newColor < finalColor){
            return finalColor;
        }
    }
    else{
        if(newColor > finalColor) {
            return finalColor;
        }
    }
    return currentColor + delta;
}

// stepRGBArray - the array containing the current steps
// fromRGB - [R,G,B] for color going from
// toRGB - [R,G,B] array for destination color
// fromA - from opacity
// toA - to opacity
// cycles - number of steps which are required
function AddStepsToColor(stepRGBArray, fromRGB, toRGB, fromA, toA, cycles) {
    var stepSizeR = getStepSize(fromRGB[0], toRGB[0], cycles);
    var stepSizeG = getStepSize(fromRGB[1], toRGB[1], cycles);
    var stepSizeB = getStepSize(fromRGB[2], toRGB[2], cycles);
    var stepSizeA = getStepSize(fromA, toA, cycles);

//    console.log("from: " + fromRGB);
//    console.log("to:   " + toRGB);
//    console.log("step: " + stepSizeR + ", " + stepSizeG + ", " + stepSizeB);

    // set starting values
    var currentR = fromRGB[0];
    var currentG = fromRGB[1];
    var currentB = fromRGB[2];
    var currentA = fromA;

    var flashStepsThisElement = [];
    for (var n = 0; n < cycles; n++) {
        // get next color step
        currentR = getStepColor(currentR, stepSizeR, cycles, n, toRGB[0]);
        currentG = getStepColor(currentG, stepSizeG, cycles, n, toRGB[1]);
        currentB = getStepColor(currentB, stepSizeB, cycles, n, toRGB[2]);
        currentA = getStepColor(currentA, stepSizeA, cycles, n, toA);

        // store as an array of colors, being a single step
        var step = [Math.ceil(currentR), Math.ceil(currentG), Math.ceil(currentB), currentA];
//        console.log(step);
        stepRGBArray.push(step);
    }

    // add final step with exact match of new color
    step = [toRGB[0], toRGB[1], toRGB[2], toA];
    console.log(step);
    stepRGBArray.push(step);
}

// add a flasher for an element.
// All color parameters are arrays in the format [R, G, B]
// "cycles" is the number of cycles to run for
function addFlasher(elementName, fromRGBBack, fromRGBFore, toRGBBack, toRGBFore, cycles) {
    var whiteRGB = [255, 255, 255];

    // add data to arrays for queueing
    var element = document.getElementById(elementName);
    flashElements.push(element);

    // Background color
    var flashStepsThisElement = [];
    // go from original color to white
    AddStepsToColor(flashStepsThisElement, fromRGBBack, whiteRGB, 1, 0, cycles);

    // go from white to destination color
    AddStepsToColor(flashStepsThisElement, whiteRGB, toRGBBack, 0, 1, cycles);
    // add to steps for background colors of elements
    flashStepsBack.push(flashStepsThisElement);

    // repeat for foreground colors
    flashStepsThisElement = [];
    AddStepsToColor(flashStepsThisElement, fromRGBFore, whiteRGB, 1, 0, cycles);
    AddStepsToColor(flashStepsThisElement, whiteRGB, toRGBFore, 1, 0, cycles);
    flashStepsFore.push(flashStepsThisElement);


    // start interval method if it doesn't exist
    if (flasher == null) {
        flasher = window.setInterval(function () {
            var somethingFlashed=false;
            var i=0;
            while (i < flashElements.length) {
                // get next elementand its associated data
                var elementToFlash = flashElements[i];
                var flashStepsThisElementBack = flashStepsBack[i];
                var flashStepBack = flashStepsThisElementBack[0];
                var flashStepsThisElementFore = flashStepsFore[i];
                var flashStepFore = flashStepsThisElementFore[0];

                // code up the colors
                var backColor = 'rgba(' + flashStepBack[0] + ',' + flashStepBack[1] + ',' + flashStepBack[2] + ',' + flashStepBack[3] + ')';
                var foreColor = 'rgb(' + flashStepFore[0] + ',' + flashStepFore[1] + ',' + flashStepFore[2] + ')';
                
                // apply colors
                elementToFlash.style.background = backColor;
                elementToFlash.style.color = foreColor;

                // remove these colors from the step arrays
                flashStepsThisElementBack.splice(0, 1);
                flashStepsThisElementFore.splice(0, 1);

                // was this the final flash?
                if(flashStepsThisElementBack.length == 0) {
                    // remove the element and its associated data from the global arrays
                    flashElements.splice(i, 1);
                    flashStepsBack.splice(i, 1);
                    flashStepsFore.splice(i, 1);
                }
                else {
                    i++;    // next element
                }
            }

            // stop timer if nothing left flashing
            if (flashElements.length==0) {
                clearInterval(flasher);
                flasher = null;
            }
        }, 1);
    }
}