// used in defining cell spec
var MAX_HP = 4;
var MAX_ENERGY = 8;
var ACIDITY = 0;
 
function decide(arena, cell, outputCallback) {

    var nearbyEmpties = arena.getAdjacentMatches(cell.point, [".", "c"]);
    var nearbyEnemies = arena.getAdjacentMatches(cell.point, ["x"]);
    var nearbyCorpses = arena.getAdjacentMatches(cell.point, ["c"]);
    var nearbyFriends = arena.getAdjacentMatches(cell.point, ["o"]);
 
    if (nearbyFriends.length >= 8) {
        outputCallback("REST");
        return;
    }

    if (nearbyFriends.length >= 7 && nearbyEnemies.length < 0 && nearbyCorpses.length > 0 && energy < MAX_ENERGY) {
        outputCallback("EAT " + arena.getDirection(cell, nearbyCorpses[(nearbyCorpses.length*Math.random())|0]));
        return;
    }

    // if you have two or more nearby enemies, explode if possible
    if(nearbyEnemies.length >= 1
        && cell.energy >= cell.hp 
        && cell.hp <= 1 
        && nearbyEnemies.length > nearbyFriends.length) {
        outputCallback("EXPLODE");
        return;
    }

    // if you have two or more nearby enemies, explode if possible
    if(nearbyEnemies.length >= 3 && cell.energy >= cell.hp && nearbyEnemies.length > nearbyFriends.length) {
        outputCallback("EXPLODE");
        return;
    }
    
    // if you have the energy and space to divide, do it
    if(cell.energy >= 5 && nearbyEmpties.length > 0) {
        var ed = arena.getEnemyDirection(cell);
        if (nearbyEmpties.indexOf(ed) >= 0 && Math.random() < 0.5){
            outputCallback("DIVIDE " + ed);
        } else{
            outputCallback("DIVIDE " + arena.getDirection(cell, nearbyEmpties[(nearbyEmpties.length*Math.random())|0]));
        }
        return;
    }

    // if at least one adjacent enemy, attack if possible
    if(cell.energy > 0 && nearbyEnemies.length > 0) {
        outputCallback("ATTACK " + arena.getDirection(cell, nearbyEnemies[(nearbyEnemies.length*Math.random())|0]) + " " + Math.min(cell.energy, 3));
        return;
    }

    if (Math.random() < 0.5) {
        for(var i=0; i<nearbyEmpties.length; ++i) {
            outputCallback("MOVE " + arena.getEnemyDirection(cell));
            return;
        }
    } 

    if (nearbyEmpties.length > 0 && nearbyEnemies.length <= 6) {
        outputCallback("REST"); // because next turn is divide time
        return;
    }

    // if there's a nearby corpse, eat it if your energy is below max
    if(nearbyCorpses.length > 0) {
        outputCallback("EAT " + arena.getDirection(cell, nearbyCorpses[(nearbyCorpses.length*Math.random())|0]));
        return;
    }
 
    outputCallback("REST");
    return;
}
 
var input = "";
// quiet stdin EPIPE errors
process.stdin.on("error", function(err) {
    console.log("slight error: " + err);
});
process.stdin.on("data", function(data) {
    input += data;
});
process.stdin.on("end", function() {
    if(input == "BEGIN") {
        // output space-separated attributes
        process.stdout.write([MAX_HP, MAX_ENERGY, ACIDITY].join(" "));
    } else {
        // read in arena and decide on an action
        var arena = new Arena();
        var lines = input.split("\n");
        var dimensions = lines[0].split(" ").map(function(d) { return parseInt(d); });
        arena.width = dimensions[0];
        arena.height = dimensions[1];
        for(var y=1; y<=dimensions[1]; ++y) {
            for(var x=0; x<lines[y].length; ++x) {
                arena.set(x, y-1, lines[y][x]);
            }
        }
 
        var stats = lines[dimensions[1]+2].split(" ");
        var cell = { x: stats[0], y: stats[1], hp: stats[2], energy: stats[3], point: arena.get(stats[0], stats[1]) };
 
        // decide on an action and write the action to stdout
        decide(arena, cell, function(output) { process.stdout.write(output); })
    }
});
 
var Arena = function() {
    this.dict = {};
};
Arena.prototype = {
    // get Point object
    get: function(x,y) {
        return this.dict[x+","+y];
    },
 
    // store Point object
    set: function(x,y,d) {
        this.dict[x+","+y] = new Point(x,y,d);
    },
 
    // get an array of all Points adjacent to this one whose symbol is contained in matchList
    // if matchList is omitted, return all Points
    getAdjacentMatches: function(point, matchList) {
        var result = [];
        for(var i=-1; i<=1; ++i) {
            for(var j=-1; j<=1; ++j) {
                var inspectedPoint = this.get(point.x+i, point.y+j);
                if(inspectedPoint && 
                   (i!=0 || j!=0) &&
                   (!matchList || matchList.indexOf(inspectedPoint.symbol) != -1)) {
                    result.push(inspectedPoint);
                }
            }
        }
        return result;
    },
 
    // return the direction from point1 to point2
    getDirection: function(point1, point2) {
        var dx = point2.x - point1.x;
        var dy = point2.y - point1.y;
        dx = Math.abs(dx) / (dx || 1);
        dy = Math.abs(dy) / (dy || 1);
	
        c2d = { "0,0":"-",
                "0,-1":"N", "0,1":"S", "1,0":"E", "-1,0":"W",
                "-1,-1":"NW", "1,-1":"NE", "1,1":"SE", "-1,1":"SW" };
 
        return c2d[dx + "," + dy];
    },

    getEnemyDirection: function(p) {
        for (var i = 0; i < this.width; i++) {
            for (var j = 0; j < this.height; j++) {
                var found = this.get(i,j);
                if (found != null && found.symbol == "x") {
                    return this.getDirection(p, found);
                }
            }
        }
        return "N"; //should never happen
    }
}
 
var Point = function(x,y,d) {
    this.x = x;
    this.y = y;
    this.symbol = d;
}
Point.prototype.toString = function() {
    return "(" + this.x + ", " + this.y + ")";
}