'use strict';

function pitaco(id) {
    let base = document.getElementById("pitaco");
    let baseurl = "https://pitaco.dalligna.com"
    if (typeof (base) === undefined)
        console.log("### PITACO is missing a div with id 'pitaco'");

    let loadComments = () => {
        let xhr = new XMLHttpRequest();
        xhr.open("POST", baseurl+"/api/getComments");
        xhr.setRequestHeader('Content-type', 'application/json');
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        xhr.overrideMimeType("text/html");
        xhr.send(`"${document.location.href}"`);
        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4 && xhr.status === 200) {
                let resp = "";
                for (let comment of JSON.parse(xhr.responseText))
                    resp += `<li><q class="text">${comment.text}</q><span class="author">${comment.author}</span></li>`;
                document.getElementById('pitaco_commentBoxLog').innerHTML = resp;
            }
        };
    };

    let postComment = () => {
        let client = new XMLHttpRequest();
        client.open("POST", baseurl+"/api/addComment");
        client.setRequestHeader('Content-type', 'application/json');
        client.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        client.overrideMimeType('text/html');
        client.send(JSON.stringify({
            wskey: id,
            url: document.location.href,
            text: document.getElementById("pitaco_comment").value,
            author: document.getElementById("pitaco_author").value,
        }));
        client.onreadystatechange = function () {
            if (client.readyState === 4 && client.status === 200) {
                document.getElementById("pitaco_comment").value = "";
                document.getElementById("pitaco_author").value = "";
                loadComments();
            }
        };
    };

    base.innerHTML = "<ul id='pitaco_commentBoxLog' class='pitaco comments'></ul>"
        + "<div id='pitaco_commentbox' class='pitaco commentbox'>"
        + " <div class='pitaco messagebox'><label for='pitaco_comment'>Message: </label><textarea id='pitaco_comment'></textarea></div>"
        + " <div class='pitaco authorbox'><label for='pitaco_author'>Author: </label><input type='text' id='pitaco_author' /></div>"
        + "</div>";

    let btn = document.createElement("button");
    btn.innerText = "Post";
    btn.onclick = postComment;
    document.getElementById("pitaco_commentbox").append(btn);

    loadComments();

}