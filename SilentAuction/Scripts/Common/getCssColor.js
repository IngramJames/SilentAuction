function getElementProperty(elementId, propertyId) {
    var element = document.getElementById(elementId);
    backColor = window.getComputedStyle(element, null).getPropertyValue(propertyId)
    return backColor;
}

function getElementColorRGB(elementId, propertyId) {
    var color = getElementProperty(elementId, propertyId);
    var rgb = getRGBFromColor(color);
    return rgb;
}

function getElementBackColor(elementId) {
    var backColor = getElementProperty(elementId, "background-color");
    return backColor;
}

function getElementBackColorRGB(elementId) {
    var backColor = getElementBackColor(elementId);
    var rgb = getRGBFromColor(backColor);
    return rgb;
}

function getElementForeColor(elementId) {
    var backColor = getElementProperty(elementId, "color");
    return backColor;
}

function getElementForeColorRGB(elementId) {
    var foreColor = getElementBackColor(elementId);
    var rgb = getRGBFromColor(foreColor);
    return rgb;
}

function getStyleRuleBackColor(ruleName) {
    var leadBidStyleRule = getStyleRuleByName(ruleName);
    var color = leadBidStyleRule.style["backgroundColor"];
    return color;
}

function getStyleRuleBackColorRGB(ruleName) {
    var color = getStyleRuleBackColor(ruleName);
    var rgb = getRGBFromColor(color);
    return rgb;
}

function getStyleRuleForeColor(ruleName) {
    var styleRule = getStyleRuleByName(ruleName);
    var color = styleRule.style["color"];
    return color;
}

function getStyleRuleForeColorRGB(ruleName) {
    var color = getStyleRuleForeColor(ruleName);
    var rgb = getRGBFromColor(color);
    return rgb;
}

function getRGBFromColor(colorString) {
    var a = document.createElement('div');
    a.style.color = colorString;
    var rgbArray = window.getComputedStyle(document.body.appendChild(a)).color.match(/\d+/g).map(function (a) { return parseInt(a, 10); });
    document.body.removeChild(a);
    return rgbArray;
}

function getStyleRuleByName(name) {
    var styleRule = null;
    var lowerName = name.toLowerCase();
    var i = 0;
    var found = false;
    while (i < document.styleSheets.length && !found) {
        var styleSheet = document.styleSheets[i];
        for (var r in styleSheet.cssRules) {
            var rule = styleSheet.cssRules[r];
            var ruleDetails = rule.cssText;
            if (ruleDetails) {
                ruleDetails = ruleDetails.toLowerCase();

                var index = ruleDetails.indexOf(lowerName);
                console.log(index);
                if (index > 0) {
                    styleRule = rule;
                    found = true;
                    break;
                }
            }
        }
        i++;
    }
    return styleRule;
}
