var pageIsAt = 1;
var maxProducts = 1;
var perPage = 1;
var language = "USD";


function QueryProducts()
{
    $("div.body-content").css("cursor", "wait");
    var searchString = $("#search-box")[0].value;
    var catID = $("#category-select")[0].value;
    perPage = $("#quantity-select")[0].value;
    var requestData = { catID: catID, searchString: searchString, perPage: perPage, page: pageIsAt };
    var response = $.ajax({
        data : requestData,
        type : 'GET',
        url : '/Home/Json/',
        dataType : 'json',
        success : function(data){ ProcessResponse(data) }
    });
}
function ProcessResponse(data)
{
    if (data.error == null)
    {
        maxProducts = data.totalResults;
        UpdateTable(data.products);
    }
    else
    {
        ShowError(data.error);
    }
}
function UpdateTable(data)
{
    ClearMessages();
    $("#products-table").replaceWith($("<table id='products-table'><tbody></tbody></table>"));
    $table = $("#products-table");
    $.each(data, function (key, value) {
        $contents = $("<tr>");
        $contents.append($("<td>").append($("<img>", {src : value.ImageURL, alt : "Preview not availible"}).addClass("preview")));
        $contents.append($("<td>", { text: value.Title }));
        $contents.append($("<td>", { text: value.Price, class : "price"}));
        $contents.append($("<td>").append($("<a>", {href : value.PageURL, text : "Details"})));
        $contents.appendTo($table);
    });
    UpdatePaging(pageIsAt);
    language = "USD";
    QueryExchangeRate();
    $("div.body-content").css("cursor", "");
}
function UpdatePaging(page)
{
    $("#paging").replaceWith("<span id='paging'>");
    $span = $("#paging");
    $span.append($("<a>", { href: "#", onclick: "SetPage(1)", text: "<" }));
    for (var i = 3; i > 0; i--)
    {
        if ((page - i) > 0)
        {
            $span.append($("<a>", { href: "#", onclick: "SetPage(" + (page - i) + ")", text: page - i }));
        }
    }
    $span.append(page.toString());
    for (var i = 1; i <= 3; i++)
    {
        if ((page + i) <= 10)
        {
            $span.append($("<a>", { href: "#", onclick: "SetPage(" + (page + i) + ")", text: page + i }));
        }
    }
    $span.append($("<a>", { href: "#", onclick: "SetPage(" + Math.min((page + 1), 10) + ")", text: ">" }));
}
function SetPage(newPage)
{
    pageIsAt = newPage;
    QueryProducts();
}
function ShowError(errorString)
{
    var $errorBox = $("<div>", { class: "alert-box error" });
    $errorBox.append($("<span>", { text: "Error: " }));
    $errorBox.append(errorString);
    $errorBox.insertAfter(".navbar");
    $("div.body-content").css("cursor", "");
}
function ClearMessages()
{
    $(".alert-box").remove();
}


// Currency conversion:
$(document).ready(function () { $("#currency-select").change(function (eventObject) { QueryExchangeRate() }) });
function QueryExchangeRate()
{
    var newLanguage = $("#currency-select").val();
    if (newLanguage != language)
    {
        $.ajax({
            url: "/Home/ExchangeRate/",
            type: "GET",
            data: { from: language, to: newLanguage },
            dataType: "json",
            success: function (data) {language = newLanguage; UpdateCurrency(data) }
        });
    }
}
function UpdateCurrency(data)
{
    $("td.price").each(function() { this.innerHTML = this.innerHTML * data.Rate });
}