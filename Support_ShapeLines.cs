///line drawing///

datablock StaticShapeData(C_SquareShape)
{
    shapeFile = "./box0.2.dts";
    //base scale of shape is .2 .2 .2
};

$defaultColor = "1 0 0 0.5";

function StaticShape::drawLine(%this, %pos1, %pos2, %color, %scale, %offset) {
	%len = vectorLen(vectorSub(%pos2, %pos1));
	if (%scale <= 0) {
		%scale = 1;
	}
	if (%color $= "" || getWordCount(%color) < 4) {
		%color = $defaultColor;
	}

    %vector = vectorNormalize(vectorSub(%pos2, %pos1));

    %xyz = vectorNormalize(vectorCross("1 0 0", %vector)); //rotation axis
    %u = mACos(vectorDot("1 0 0", %vector)) * -1; //rotation value

    %this.setTransform(vectorScale(vectorAdd(%pos1, %pos2), 0.5) SPC %xyz SPC %u);
    %this.setScale((%len/2 + %offset) SPC %scale SPC %scale);
    %this.setNodeColor("ALL", %color);
    if (getWord(%color, 3) < 1) {
    	%this.startFade(0, 0, 1);
    } else {
    	%this.startFade(0, 0, 0);
    }

    return %this;
}

function StaticShape::createBoxAt(%this, %pos, %color, %scale) {
    if (%scale <= 0) {
        %scale = 1;
    }
    if (%color $= "" || getWordCount(%color) < 4) {
        %color = $defaultColor;
    }

    %this.setTransform(%pos SPC "1 0 0 0");
    %this.setScale(%scale SPC %scale SPC %scale);
    %this.setNodeColor("ALL", %color);
    if (getWord(%color, 3) < 1) {
        %this.startFade(0, 0, 1);
    } else {
        %this.startFade(0, 0, 0);
    }
}

function drawLine(%start, %end, %color, %radius) {
    if (%width < 0.05) {
        %width = 0.05;
    }

    if (%color $= "") {
        %color = $defaultColor;
    }
    %shape = new StaticShape(ShapeLines) {
        datablock = C_SquareShape;
    };
    return %shape.drawLine(%start, %end, %color, %width, 0);
}

function clearLines() {
    while (isObject(ShapeLines)) {
        ShapeLines.delete();
    }
}

function getCompassVec(%cl) {
    %forwardVec = vectorNormalize(getWords(%cl.getControlObject().getForwardVector(), 0, 1) SPC 0);
    %xOry = mAbs(getWord(%forwardVec, 0)) - mAbs(getWord(%forwardVec, 1));
    if (%xOry > 0) {
        %compassVec = mFloatLength(getWord(%forwardVec, 0), 0) SPC "0 0";
    } else {
        %compassVec = 0 SPC mFloatLength(getWord(%forwardVec, 1), 0) SPC 0;
    }
    return %compassVec;
}