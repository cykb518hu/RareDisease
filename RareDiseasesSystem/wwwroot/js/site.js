// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var main;
$("#component-template").load("/js/components.html", function (response) {
    $("#component-template").html(response);
    main = new Vue({
        'el': '#mainArea',
        data: {
            loginUser: loginUser,
            menuActiveIndex: menuActiveIndex //顶部导航
        },
        computed: {

        },
        mounted: function () {

        },
        watch: {

        },
        created: function () {
        },
        methods: {
       
        }
    });
});
