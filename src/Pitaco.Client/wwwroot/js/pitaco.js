'use strict';

let pitaco = (id) => {
    let base = document.getElementById("pitaco");
    if(typeof(base) === undefined)
        console.log("### PITACO is missing a div with id 'pitaco'");

    let loadComments = () => {
        let xhr = new XMLHttpRequest();
        xhr.open("POST", "https://localhost:5001/api/getComments");
        xhr.setRequestHeader('Content-type', 'application/json')
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        xhr.overrideMimeType("text/html");
        xhr.send(`"${id}"`)
        xhr.onreadystatechange = function() {
            if(xhr.readyState === 4 && xhr.status === 200) {
                let resp = "";
                for(let comment of JSON.parse(xhr.responseText))
                    resp += `<li>${comment.text} - ${comment.author}</li>`;
                document.getElementById('pitaco_commentBoxLog').innerHTML = resp;
            }
        }
    }

    let postComment = () => {
        let client = new XMLHttpRequest();
        client.open("POST", "https://localhost:5001/api/addComment");
        client.setRequestHeader('Content-type', 'application/json')
        client.setRequestHeader('X-Requested-With', 'XMLHttpRequest')
        client.overrideMimeType('text/html')
        client.send(JSON.stringify({
            wskey: id,
            url: "localhost:5001",
            text: document.getElementById("pitaco_comment").value,
            author: document.getElementById("pitaco_author").value
        }));
        client.onreadystatechange = function() {
            if(client.readyState === 4 && client.status === 200) {
                document.getElementById("pitaco_comment").value = ""
                document.getElementById("pitaco_author").value = ""
                loadComments();
            }
        }
    }

    base.innerHTML = "<ul id='pitaco_commentBoxLog'></ul>"
        + "Message: <br><textarea id='pitaco_comment'></textarea><br>"
        + "Author: <br><input type='text' id='pitaco_author' />";

    let btn = document.createElement("button");
    btn.innerText= "Post";
    btn.onclick = postComment
    base.append(btn);

    loadComments();

}