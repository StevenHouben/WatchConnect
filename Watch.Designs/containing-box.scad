module box()
{
		difference() {
				cube([30, 30, 5], center = true);
				cube([28, 28, 30], center = true);
				translate([0, -14, 0])
				{
					cube([4, 3, 4], center=true);
				}
				translate([8, -14, 0])
				{
					cube([4, 3, 4], center=true);
				}
				translate([-8, -14, 0])
				{
					cube([4, 3, 4], center=true);
				}
				translate([0, 15, 0])
				{
					cube([4, 3, 4], center=true);
				}
				translate([8, 15, 0])
				{
					cube([4, 3, 4], center=true);
				}
				translate([-8, 15, 0])
				{
					cube([4, 3, 4], center=true);
				}
		}	
		translate([0, 0, -2])
		{
			cube([30, 30, 1], center=true);
		}
	
	
}

box();

