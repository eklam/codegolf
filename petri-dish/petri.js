/*
Node.js implementation of the "Battle for the Petri Dish" driver
 
Copyright (c) 2014, Andrew P. Sillers
All rights reserved.
 
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 
1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 
2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
var colors = require('colors');
 
function World(height, width, exec1, exec2, victoryCallback) {
    var that = this;
    this.grid = {};
 
    // executable paths
    this.exec1 = exec1;
    this.exec2 = exec2;
    this.victoryCallback = victoryCallback;
 
    this.activePlayer = 1;
 
    this.width = width;
    this.height = height;
 
    this.cells = { 1: [], 2: [] };
    this.celldata = {};
 
    // init world grid
    for(var y=0; y<height; ++y) {
	for(var x=0; x<width; ++x) {
	    this.grid[x+","+y] = null;
	}
    }
 
    this.init(function(err) {
	if(err) { throw err; }
        that.launch();
    });
 
}
 
World.prototype = {
    init: function(callback) {
	var that = this;
        (function initPlayer(pNum) {
            if(pNum > 2) { callback(); return; }
 
	    runProcessWithInput(that["exec"+pNum], "BEGIN", function(output) {
                var props = output.split(" ");
		console.log("Player " + pNum + " stats: " + props);
                that.celldata[pNum] = {
                    team: pNum,
                    hp: parseInt(props[0], 10),
                    energy: parseInt(props[1], 10),
                    acidity: parseInt(props[2], 10)
                };
		var data = that.celldata[pNum];
                if(data.hp + data.energy + data.acidity != 12 ||
                   data.acidity < 0 || data.energy < 0 || data.hp < 0) {
            console.log("player " + pNum + "hp: " + data.hp + "energy: " +data.energy+ "acid: " + data.acidity);
		    callback("player " + pNum + " messed up initial stats: " + props);
		} else {
                    initPlayer(pNum + 1);
		}
	    });
 
        })(1);
    },
    
    launch: function() {
	var that = this;
 
	// create starter cells
        this.addCell(1, 1, 1);
 
        var x = this.width-2, y = this.height-2;
        this.addCell(x, y, 2);
 
	var turnCount = 0;
 
	(function takeTurn(pNum) {
	    turnCount++;
	    //console.log("Turn #" + turnCount);
	    var cellList = that.cells[pNum];
	    var indexOverstep = cellList.length;
 
	    (function runCell(cellIndex) {
		if(cellIndex == indexOverstep) {
		    that.turnCleanup(turnCount);
		    if("winner" in that) {
			that.victoryCallback(that.winner);
			return;
		    }
 
		    process.nextTick(function() { takeTurn(pNum==1?2:1); });
		    return;
		}
 
        if (pNum == 2){
		//console.log("running cell #" + (cellIndex+1) + "/" + indexOverstep);
		console.log(that.outputStringForCell2(cellList[cellIndex]));
		//that.outputStringForCell2(cellList[cellIndex]);
        }
 
		that.invokeCell(cellList[cellIndex], function() {
		    that.checkWinner();
		    if(!("winner" in that)) {
			runCell(cellIndex+1);
		    } else {
			that.victoryCallback(that.winner);
		    }
		});
	    })(0);
	})(1);
    },
 
    invokeCell: function(cell, callback) {
	if(cell.hp <= 0) { callback(); return; }
 
	var that = this;
        runProcessWithInput(that["exec"+cell.team], this.outputStringForCell(cell), function(output) {
            that.execCommand(cell, strip(output));
	    callback();
        });
    },
 
    // remove dead cells from the team roster
    turnCleanup: function(turnCount) {
	var losers = {};
	for(var team=1; team<=2; ++team) {
	    for(var i=this.cells[team].length-1; i>=0; --i) {
		if(this.cells[team][i].hp <= 0) {
		    this.cells[team].splice(i, 1);
		}
	    }
	}
 
	var p1count = this.cells[1].length;
	var p2count = this.cells[2].length;
	var isEndGame = turnCount == 300;
 
	if(isEndGame) {
	    if(p1count == p2count) {
		this.winner = 0;
	    } else if(p1count > p2count) {
		this.winner = 1;
	    } else {
		this.winner = 2;
	    }
	}
    },
 
    checkWinner: function() {
	var p1alive = this.cells[1].some(function(c) { return c.hp > 0; });
	var p2alive = this.cells[2].some(function(c) { return c.hp > 0; });
 
	if(!p1alive && !p2alive) {
	    this.winner = 0;
	} else if(!p2alive) {
	    this.winner = 1;
	} else if(!p1alive) {
	    this.winner = 2;
	}
    },
 
    addCell: function(x, y, team, energy) {
        this.grid[x+","+y] = new Cell(x, y, this.celldata[team], energy);
	this.cells[team].push(this.grid[x+","+y]);
    },
 
    killCell: function(cell) {
	this.grid[cell.x+","+cell.y] = "c";
    },
 
    execCommand: function(cell, commandStr) {
    
	// console.log("execing command: " + commandStr);
	var commandArray = commandStr.split(" ");
	commandArray[0] = commandArray[0].toUpperCase();
	commandArray[1] = commandArray[1] && commandArray[1].toUpperCase();
	
	var vector, dest;
	if(commandArray[1]) { vector = directionToVector(commandArray[1]); }
	if(vector) { dest = { x: cell.x + vector.x, y: cell.y + vector.y } };
 
	var strength = parseInt(commandArray[2], 10);
 
	switch(commandArray[0]) {
	case "MOVE":
	    if(cell.energy < 1 || !vector || !this.isFree(dest.x, dest.y)) {
		cell.addEnergy(2);
	    } else {
		this.grid[cell.x+","+cell.y] = null;
		cell.x = dest.x;
		cell.y = dest.y;
		this.grid[dest.x+","+dest.y] = cell;
	    }
	    break;
	case "DIVIDE":
	    if(cell.energy < 5 || !vector || !this.isFree(dest.x, dest.y)) {
		cell.addEnergy(2);
	    } else {
		cell.energy -= 5;
		this.addCell(dest.x, dest.y, cell.team, cell.energy);
	    }
	    break;
	case "ATTACK":
	    if(strength > 3 || strength < 1 || isNaN(strength) || cell.energy < strength || !vector || !this.isCell(dest.x, dest.y)) {
		cell.addEnergy(2);
	    } else {
		cell.energy -= strength;
		var target = this.grid[dest.x+","+dest.y];
		target.hp -= strength;
		if(target.hp <= 0) { this.killCell(target); }
	    }
	    break;
	case "EAT":
	    if(!vector || !this.isCorpse(dest.x, dest.y)) {
		cell.addEnergy(2);
	    } else {
		cell.addEnergy(4);
		this.grid[dest.x+","+dest.y] = null;
	    }
	    break;
	case "EXPLODE":
	    if(cell.hp > 3 || cell.energy <= cell.hp) {
		cell.addEnergy(2);
	    } else {
		for(var dx=-1; dx<=1; ++dx) {
		    for(var dy=-1; dy<=1; ++dy) {
			dest = { x: cell.x + dx, y: cell.y + dy };
			if(this.isCell(dest.x, dest.y)) {
			    var target = this.grid[dest.x+","+dest.y];
			    target.hp -= cell.hp;
			    if(target.hp <= 0) { this.killCell(target); }
			}
		    }
		}
	    }
	    break;
	default:
	    cell.addEnergy(2);
	}
    },
 
    outputStringForCell: function(cell) {
		var output = this.width + " " + this.height + "\n";
		for(var y=0; y<this.height; ++y) {
		    for(var x=0; x<this.width; ++x) {
			var here = this.grid[x+","+y];
			if(here instanceof Cell) { output += cell.team==here.team?"o":"x"; }
			else if(here == "c") { output += "c"; }
			else if(here == null) { output += "."; }
		    }
		    output += "\n";
		} 
	 
		output += "\n" + [cell.x, cell.y, cell.hp, cell.energy].join(" ");
	 
		return output;	
    },

    outputStringForCell2: function(cell) {
		var output = this.width + " " + this.height + "\n";
		for(var y=0; y<this.height; ++y) {
		    for(var x=0; x<this.width; ++x) {
			var here = this.grid[x+","+y];
			if(here instanceof Cell) {
				if (cell.team==here.team) {
					output += colors.red("o");
				}else{
					output += colors.green("x");
				}
				
			}
			else if(here == "c") { output += "c"; }
			else if(here == null) { output += "."; }
		    }
		    output += "\n";
		} 
	 
		output += "\n" + [cell.x, cell.y, cell.hp, cell.energy].join(" ");
	 
		return output;	
    },
 
    isCell: function(x, y) {
        return this.grid[x+","+y] instanceof Cell;
    },
 
    isCorpse: function(x, y) {
        return this.grid[x+","+y] == "c";
    },
 
    // if space exists and it's not a cell, it free for movement/division
    isFree: function(x, y) {
	return this.grid.hasOwnProperty(x+","+y) && !(this.grid[x+","+y] instanceof Cell);
    }
}
 
function strip(str) {
    if(str[str.length-1] == "\n") {
	return str.substr(0, str.length-1);
    }
    return str;
}
 
function runProcessWithInput(processName, input, outputCallback) {
    var spawn = require('child_process').spawn;
    var output = "";
    var args = processName.split(" ");
    var spawnedProc = spawn(args.splice(0, 1)[0], args);
    
    spawnedProc.stdin.end(input);
    
    // quiet stdin EPIPE errors
    spawnedProc.stdin.on("error", function(err) {
	//console.log("slight error: " + err);
    });
    
    // output child script errors
    spawnedProc.stderr.on("data", function(data) {
        console.error(data.toString());
    });
    
    spawnedProc.stdout.on("data", function(data) {
        output += data;
    });
    spawnedProc.on("close", function() {
	output = strip(output);
	outputCallback(output);
    });	    
}
 
function directionToVector(dStr) {
    if(["N","S","E","W","NE","NW","SE","SW"].indexOf(dStr) == -1) { return false; }
 
    var vector = { x:0, y:0 };
 
    if(dStr.indexOf("N") != -1) { vector.y = -1; }
    if(dStr.indexOf("S") != -1) { vector.y = 1; }
    if(dStr.indexOf("E") != -1) { vector.x = 1; }
    if(dStr.indexOf("W") != -1) { vector.x = -1; }
 
    return vector;
}
 
function Cell(x, y, props, energy) {
    this.team = props.team;
    this.x = x;
    this.y = y;
    this.hp = props.hp;
    this.energy = energy!=undefined?energy:props.energy;
    this.maxEnergy = props.energy;
    this.acidity = props.acidity;
}
 
Cell.prototype.addEnergy = function(eBoost) {
    this.energy = Math.min(this.maxEnergy, this.energy + eBoost);
}
 
var driver1 = process.argv[2];
var driver2 = process.argv[3];
 
if(!driver1 || !driver2) {
    console.log("Usage: node petri.js './first driver' './second driver'");
    process.exit();
}
 
new World(4, 10, driver1, driver2, function(winner) {
    if(!winner) { console.log("Draw"); }
    else { console.log("Player " + winner + " wins"); }
});