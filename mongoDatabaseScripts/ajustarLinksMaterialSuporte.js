const MongoClient = require('mongodb').MongoClient;
const assert = require('assert');

// documentaçÃo base
// https://github.com/mongodb/node-mongodb-native

// Connection URL
// const url = 'mongodb://dev:tecs638form@ds113732.mlab.com:13732/academia-btg-bkp';
// const url = 'mongodb://dev:tecs638form@ds161312.mlab.com:61312/academia-btg-bkp-03-12';
const url = 'mongodb://dev:tecs638form@ds147616-a0.mlab.com:47616,ds147616-a1.mlab.com:47616/academia-btg?replicaSet=rs-ds147616'; // 

// Database Name
const dbName = 'academia-btg';

var tablename = 'bdq_new';

async function getMongo(url, dbName) {
    // // Use connect method to connect to the server
    var client = await MongoClient.connect(url);

    console.log("Connected successfully to server");

    const db = client.db(dbName);
    //console.log(db);

    var collection = db.collection('Modules');
    // Find some documents
    var modules = await collection.find({}).toArray();

    modules.forEach(function (mod, index) {
        var update = false;
        if(mod.instructorImageUrl && mod.instructorImageUrl.indexOf("http://dev.academia.api.tg4.com.br") >= 0){
            var before = mod.instructorImageUrl;
            mod.instructorImageUrl = mod.instructorImageUrl.replace("http://dev.academia.api.tg4.com.br", "https://btg.api.academia.proseek.com.br");
            console.log("instructor - " + before + " -> " + mod.instructorImageUrl);
            update = true;
        }
        if(mod.imageUrl && mod.imageUrl.indexOf("http://dev.academia.api.tg4.com.br") >= 0){
            var before = mod.imageUrl;
            mod.imageUrl = mod.imageUrl.replace("http://dev.academia.api.tg4.com.br", "https://btg.api.academia.proseek.com.br");
            console.log("image - " + before + " -> " + mod.imageUrl);
            update = true;
        }
        mod.supportMaterials.forEach(element => {
            if(element.downloadLink && element.downloadLink.indexOf("http://dev.academia.api.tg4.com.br") >= 0){
                var before = element.downloadLink;
                element.downloadLink = element.downloadLink.replace("http://dev.academia.api.tg4.com.br", "https://btg.api.academia.proseek.com.br");
                console.log("supMaterial - " + before + " -> " + element.downloadLink);
                update = true;
            }
        });
        if(update){
            // collection.updateOne({
            //     _id: mod._id
            // }, {
            //     $set: {
            //         instructorImageUrl: mod.instructorImageUrl,
            //         imageUrl: mod.imageUrl,
            //         //supportMaterials: mod.supportMaterials
            //     }
            // }, function (err, result) {
            //     assert.equal(err, null);
            //     console.log(index);
            // });
        }
    });
    collection = db.collection('Events');
    // Find some documents
    var events = await collection.find({}).toArray();

    events.forEach(function (mod, index) {
        var update = false;
        if(mod.instructorImageUrl && mod.instructorImageUrl.indexOf("http://dev.academia.api.tg4.com.br") >= 0){
            var before = mod.instructorImageUrl;
            mod.instructorImageUrl = mod.instructorImageUrl.replace("http://dev.academia.api.tg4.com.br", "https://btg.api.academia.proseek.com.br");
            console.log("ev instructor - " + before + " -> " + mod.instructorImageUrl);
            update = true;
        }
        if(mod.imageUrl && mod.imageUrl.indexOf("http://dev.academia.api.tg4.com.br") >= 0){
            var before = mod.imageUrl;
            mod.imageUrl = mod.imageUrl.replace("http://dev.academia.api.tg4.com.br", "https://btg.api.academia.proseek.com.br");
            console.log("image - " + before + " -> " + mod.imageUrl);
            update = true;
        }
        mod.supportMaterials.forEach(element => {
            if(element.downloadLink && element.downloadLink.indexOf("http://dev.academia.api.tg4.com.br") >= 0){
                var before = element.downloadLink;
                element.downloadLink = element.downloadLink.replace("http://dev.academia.api.tg4.com.br", "https://btg.api.academia.proseek.com.br");
                console.log("ev supMaterial - " + before + " -> " + element.downloadLink);
                update = true;
            }
        });
        if(update){
            // collection.updateOne({
            //     _id: mod._id
            // }, {
            //     $set: {
            //         instructorImageUrl: mod.instructorImageUrl,
            //         imageUrl: mod.imageUrl,
            //         //supportMaterials: mod.supportMaterials
            //     }
            // }, function (err, result) {
            //     assert.equal(err, null);
            //     console.log(index);
            // });
        }
    });

    //console.log(excel);

    console.log("acabou");

}
getMongo(url, dbName);

