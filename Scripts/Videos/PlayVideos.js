


$(document).ready(function () {
   
    var token = $('input[name="__RequestVerificationToken"]').val();
    var Vid = document.getElementById("ObjectToDisplay_VideoId").value;
    
    $.ajax({          
        url: "/Upload/SignVideoURL",
        type: "post",
        data: {
            vUID: Vid,
            __RequestVerificationToken: token
        },
        success: (function (url) {
           
            if (url != null) {
                let newUrl = 'https://customer-4r7h3gkxcjwuyp34.cloudflarestream.com/' + url + "/iframe";
               /* let newurl = 'https://customer-4r7h3gkxcjwuyp34.cloudflarestream.com/' + url +'/manifest/video.m3u8';*/
                $("#iFrameMovie").attr('src', newUrl);
           
               
                
            }
        })
    });

    
});
