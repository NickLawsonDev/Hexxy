﻿using Microsoft.Xna.Framework;

public struct Rectf {
	
	public static readonly Rectf zero = new Rectf(0,0,0,0);
	public static readonly Rectf one = new Rectf(1,1,1,1);

	public float x,y,width,height;

	public Rectf(float x, float y, float width, float height) {
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}

	public float left {
		get {
			return x;}
	}

	public float right {
		get {
			return x+width;
		}
	}

	public float top {
		get {
			return y;
		}
	}
	
	public float bottom {
		get {
			return y+height;
		}
	}

	public Vector2 topLeft {
		get {
			return new Vector2(left, top);
		}
	}

	public Vector2 bottomRight {
		get {
			return new Vector2(right, bottom);
		}
	}
}