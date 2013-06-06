///#source 1 1 /Scripts/js/Pager3.js
$(function () {
    $.gotoPage = function (e, pg) {
        var f = $(e).closest('form');
        $("#Page", f).val(pg);
        return $.getTable(f);
    };
    $.setPageSize = function (ev, size) {
        var f = $(ev).closest('form');
        $('#Page', f).val(1);
        $("#PageSize", f).val(size);
        return $.getTable(f);
    };
//    $.getTable = function (f) {
//        var q;
//        if (f)
//            q = f.serialize();
//        $.post(f.attr('action'), q, function (ret) {
//            $(f).html(ret).ready(function () {
//                $('table.grid > tbody > tr:even', f).addClass('alt');
//                $("a.trigger-dropdown", f).dropdown();
//                $('.bt').button();
//                $(".datepicker").datepicker();
//            });
//        });
//        return false;
//    };
    $('table.grid > thead a.sortable').live("click", function () {
        var f = $(this).closest("form");
        var newsort = $(this).text();
        var sort = $("#Sort", f);
        var dir = $("#Direction", f);
        if ($(sort).val() == newsort && $(dir).val() == 'asc')
            $(dir).val('desc');
        else
            $(dir).val('asc');
        $(sort).val(newsort);
        $.getTable(f);
        return false;
    });
    $.showTable = function (f) {
        if ($('table.table', f).size() == 0)
            $.getTable(f);
        return false;
    };
    $.updateTable = function (f) {
        if ($('table.table', f).size() > 0)
            $.getTable(f);
        return false;
    };
    $("body").on("click", "input[name='toggletarget']", function (ev) {
        if ($('a.target[target="people"]').length == 0) {
            $("a.target").attr("target", "people");
            $("input[name='toggletarget']").attr("checked", true);
        } else {
            $("a.target").removeAttr("target");
            $("input[name='toggletarget']").removeAttr("checked");
        }
    });
});

///#source 1 1 /Scripts/js/headermenu2.js
$(function () {
    //$("a.trigger-dropdown").dropdown();
    $('#SearchText').each(function () {
        var searchterm = "";
        $(this).autocomplete({
            appendTo: "#SearchResults",
            position: { my: "right top", at: "right bottom", of: $("#SearchText") },
            minLength: 3,
            autoFocus: true,
            open: function () {
                $("#SearchResults > ul").css("z-index", 1002);
            },
            close: function (event) {
                var thisval = $(this).val();
                if (searchterm !== thisval && thisval !== "") {
                    return $("#SearchText").autocomplete("search");
                }
                $("#SearchText").val('');
            },
            source: function (request, response) {
                if (request.term === '---')
                    response([
                        { id: -1, line1: "People Search" },
                        { id: -2, line1: "Advanced Search" },
                        { id: -3, line1: "Organization Search" }
                    ]);
                else {
                    searchterm = request.term;
                    $.post("/Home/Names", request, function (ret) {
                        response(ret.slice(0, 15));
                    }, "json");
                }
            },
            select: function (event, ui) {
                var thisval = $(this).val();
                if (searchterm !== thisval && thisval !== "") {
                    return false;
                }
                if (ui.item.id === -1)
                    window.location = "/PeopleSearch?name=" + searchterm;
                else if (ui.item.id === -2)
                    window.location = "/QueryBuilder/Main";
                else if (ui.item.id === -3)
                    window.location = "/OrgSearch";
                else
                    window.location = (ui.item.isOrg ? "/Organization/Index/" : "/Person/Index/") + ui.item.id;
                return true;
            },
            focus: function (ev) {
                ev.preventDefault();
            }
        }).data("uiAutocomplete")._renderItem = function (ul, item) {
            if (item.id === 0)
                return $("<li>").append("<hr/>").appendTo(ul);
            var li = "<a><b>" + (item.isOrg ? "Org: " : "") + item.line1 + "</b>";
            if (item.id > 0)
                li += "<br>" + (item.isOrg ? "Div: " : "") + item.line2;
            li += "</a>";
            return $("<li>")
                .append(li)
                .appendTo(ul);
        };
    });
    $("a.tutorial").click(function (ev) {
        ev.preventDefault();
        startTutorial($(this).attr("href"));
    });
    //	$('#AddDialog').dialog({
    //		bgiframe: true,
    //		autoOpen: false,
    //		width: 750,
    //		height: 700,
    //		modal: true,
    //		overlay: {
    //			opacity: 0.5,
    //			background: "black"
    //		}, close: function () {
    //			$('iframe', this).attr("src", "");
    //		}
    //	});
    //	$('#addpeople').click(function (e) {
    //		e.preventDefault();
    //		var d = $('#AddDialog');
    //		$('iframe', d).attr("src", "/SearchAdd?type=addpeople");
    //		d.dialog("option", "title", "Add People");
    //		d.dialog("open");
    //	});
    //	$('#addorg').click(function (e) {
    //		e.preventDefault();
    //		var d = $('#AddDialog');
    //		$('iframe', d).attr("src", "/AddOrganization");
    //		d.dialog("option", "title", "Add Organization");
    //		d.dialog("open");
    //	});
    $('#cleartag').click(function (e) {
        e.preventDefault();
        if (confirm("are you sure you want to empty the active tag?"))
            $.post("/Tags/ClearTag", {}, function () {
                window.location.reload();
            });
    });
    $('.warntip').tooltip({
        delay: 150,
        showBody: "|",
        showURL: false
    });
    $.QueryString = function (q, item) {
        var r = new Object();
        $.each(q.split('&'), function () {
            var kv = this.split('=');
            r[kv[0]] = kv[1];
        });
        return r[item];
    };
    $.block = function (message) {
        if (!message)
            message = '<h1>working on it...</h1>';
        $.blockUI({
            message: message,
            overlayCSS: { opacity: 0 },
            css: {
                border: 'none',
                padding: '15px',
                backgroundColor: '#000',
                '-webkit-border-radius': '10px',
                '-moz-border-radius': '10px',
                opacity: .5,
                color: '#fff'
            }
        });
    };
    $.unblock = function () {
        $.unblockUI();
    };
    $.navigate = function (url, data) {
        url += (url.match(/\?/) ? "&" : "?") + data;
        window.location = url;
    };
    $.DateValid = function (d, growl) {
        var reDate = /^(0?[1-9]|1[012])[\/-](0?[1-9]|[12][0-9]|3[01])[\/-]((19|20)?[0-9]{2})$/i;
        if ($.dtoptions.format.startsWith('d'))
            reDate = /^(0?[1-9]|[12][0-9]|3[01])[\/-](0?[1-9]|1[012])[\/-]((19|20)?[0-9]{2})$/i;
        var v = true;
        if (!reDate.test(d)) {
            if (growl == true)
                $.growlUI("error", "enter valid date");
            v = false;
        }
        return v;
    };
    jQuery.fn.center = function (parent) {
        if (parent) {
            parent = this.parent();
        } else {
            parent = window;
        }
        this.css({
            "position": "absolute",
            "top": ((($(parent).height() - this.outerHeight()) / 2) + $(parent).scrollTop() + "px"),
            "left": ((($(parent).width() - this.outerWidth()) / 2) + $(parent).scrollLeft() + "px")
        });
        return this;
    };
    $('#SearchText').each(function () {
        $(this).addClass('text-label');
        $(this).focus(function () {
            if (this.value === '' || this.value === $(this).attr('default')) {
                this.value = '';
                $(this).removeClass('text-label');
                $(this).autocomplete("search", "---");
            }
        });
        $(this).blur(function () {
            if (this.value === '' && $(this).attr('default')) {
                this.value = $(this).attr('default');
                $(this).addClass('text-label');
            }
        });
    });
    $.fn.alert = function (message) {
        this.html('<div class="alert"><a class="close" data-dismiss="alert">×</a><span>' + message + '</span></div>');
    };
});

function dimOff() {
    $("#darkLayer").hide();
}
function dimOn() {
    $("#darkLayer").show();
}
String.prototype.startsWith = function (t, i) {
    return (t == this.substring(0, t.length));
};
String.prototype.appendQuery = function (q) {
    if (this && this.length > 0)
        if (this.contains("&") || this.contains("?"))
            return this + '&' + q;
        else
            return this + '?' + q;
    return q;
};
String.prototype.contains = function (it) {
    return this.indexOf(it) != -1;
};
String.prototype.endsWith = function (t, i) {
    return (t == this.substring(this.length - t.length));
};
String.prototype.addCommas = function () {
    var x = this.split('.');
    var x1 = x[0];
    var x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    return x1 + x2;
};

///#source 1 1 /Scripts/js/form-ajax.js
$(function () {
    $.AttachFormElements = function (f) {
        $("input.ajax-typeahead", f).typeahead({
            minLength: 3,
            source: function (query, process) {
                return $.ajax({
                    url: $(this.$element[0]).data("link"),
                    type: 'post',
                    data: { query: query },
                    dataType: 'json',
                    success: function (jsonResult) {
                        return typeof jsonResult == 'undefined' ? false : process(jsonResult);
                    }
                });
            }
        });
        $("select", f).chosen();
        $(".date", f).datepicker();
    };
    $("div.modal form.ajax").live("submit", function (event) {
        var $form = $(this);
        var $target = $form.closest("div.modal");
        $.ajax({
            type: 'POST',
            url: $form.attr('action'),
            data: $form.serialize(),
            success: function (data, status) {
                //$target.removeClass("fade");
                $target.html(data);
                var top = ($(window).height() - $target.height()) / 2;
                if (top < 10)
                    top = 10;
                $target.css({ 'margin-top': top, 'top': '0' });
                $.AttachFormElements($form);
            }
        });
        event.preventDefault();
    });
    $("form.ajax a.ajax").live("click", function (event) {
        var $this = $(this);
        var $form = $this.closest("form.ajax");
        var $modal = $form.closest("div.modal");
        var url = $this[0].href;
        var data = $form.serialize();
        $.ajax({
            type: 'POST',
            url: url,
            data: data,
            success: function (data, status) {
                if ($modal.length > 0) {
                    //$modal.removeClass("fade");
                    $modal.html(data);
                    var top = ($(window).height() - $modal.height()) / 2;
                    if (top < 10)
                        top = 10;
                    $modal.css({ 'margin-top': top, 'top': '0' });
                } else {
                    $form.html(data);
                }
                $.AttachFormElements($form);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert(xhr.status);
                alert(thrownError);
            }
        });
        event.preventDefault();
    });
    $("body").on({
        ajaxStart: function () {
           $("#loading-indicator").css({ 
               'position': 'absolute',
               'left': $(window).width() / 2, 
               'top': $(window).height() / 2,
               'z-index' : 2000
           }).show();
        },
        ajaxStop: function () {
            $("#loading-indicator").hide();
        }
    });
});

///#source 1 1 /Scripts/Search/SearchAdd.js
$(function () {
    $("a.searchadd").live("click", function (ev) {
        ev.preventDefault();
        $("<div id='search-add' class='modal fade hide' data-width='600' data-keyboard='false' data-backdrop='static' />")
            .load($(this).attr("href"), {}, function () {
                $(this).modal("show");
                $(this).on('hidden', function () {
                    $(this).remove();
                });
            });
    });
    $("#search-add a.clear").live('click', function (ev) {
        ev.preventDefault();
        $("#name").val('');
        $("#phone").val('');
        $("#address").val('');
        $("#dob").val('');
        return false;
    });

    $("form.ajax tbody > tr a.reveal").live("click", function (e) {
        e.stopPropagation();
    });
    $.NotReveal = function (ev) {
        if ($(ev.target).is("a"))
            if (!$(ev.target).is('.reveal'))
                return true;
        return false;
    };
    $("form.ajax tr.section.notshown").live("click", function (ev) {
        if ($.NotReveal(ev)) return;
        ev.preventDefault();
        ev.stopPropagation();
        $(this).removeClass("notshown").addClass("shown");
        $(this).nextUntil("tr.section").find("div.collapse")
            .off("hidden")
            .on("hidden", function (e) { e.stopPropagation(); })
            .collapse('show');
    });
    $("form.ajax tr.section.shown").live("click", function (ev) {
        if ($.NotReveal(ev)) return;
        ev.preventDefault();
        $(this).nextUntil("tr.section").find("div.collapse")
            .off("hidden")
            .on("hidden", function (e) { e.stopPropagation(); })
            .collapse('hide');
        $(this).removeClass("shown").addClass("notshown");
    });
    $('form.ajax a[rel="reveal"]').live("click", function (ev) {
        ev.preventDefault();
        $(this).parents("tr").next("tr").find("div.collapse")
            .off('hidden')
            .on("hidden", function (e) { e.stopPropagation(); })
            .collapse("toggle");
    });
    $("form.ajax tr.master").live("click", function (ev) {
        if ($.NotReveal(ev)) return;
        ev.preventDefault();
        $(this).next("tr").find("div.collapse")
            .off('hidden')
            .on("hidden", function (e) { e.stopPropagation(); })
            .collapse("toggle");
    });
    $("form.ajax tr.details").live("click", function (ev) {
        if ($.NotReveal(ev)) return;
        ev.preventDefault();
        ev.stopPropagation();
        $(this).find("div.collapse")
            .off("hidden")
            .on("hidden", function (e) { e.stopPropagation(); })
            .collapse('hide');
    });
});
