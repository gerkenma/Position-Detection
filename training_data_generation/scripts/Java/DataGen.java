package datagen;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.PrintWriter;
import java.time.LocalDateTime;

import org.hipparchus.geometry.euclidean.threed.Vector3D;
import org.orekit.bodies.*;
import org.orekit.data.DataProvidersManager;
import org.orekit.data.DirectoryCrawler;
import org.orekit.frames.FramesFactory;
import org.orekit.time.AbsoluteDate;
import org.orekit.time.TimeScalesFactory;
import org.orekit.utils.IERSConventions;
import org.orekit.frames.ITRFVersion;


public class DataGen {

	public static void main(String[] args) {
		
		//Code block to establish a path to an ephemeride data directory, as well as search through it to find the appropriate data
		File orekitData = new File("/Users/rperl/eclipse-workspace/orekit-data");
		DataProvidersManager manager = DataProvidersManager.getInstance();
		manager.addProvider(new DirectoryCrawler(orekitData));
		
		
		//Main block of program - generates coordinates for a celestial body at a given time in a certain coordinate frame (ITRf currently) and outputs
		//them to a csv file
		try(PrintWriter writer = new PrintWriter(new File("MoonData.csv"))){
		
			//datetime variable - change this to change the start date/time for the generated csv file
			LocalDateTime datetime = LocalDateTime.of(2019, 1, 1, 0, 0, 0);
			
			//Orekit's version of date/time - will work off of the date time variable
			AbsoluteDate date = new AbsoluteDate(2019, 1, 1, 0, 0, 0.0, TimeScalesFactory.getUTC());
			
			
			//Instances of the CelestialBody class for the sun and moon
			CelestialBody sun = CelestialBodyFactory.getSun();
			CelestialBody moon = CelestialBodyFactory.getMoon();
			
			//Main method of the program - takes the date variable, as well as a coordinate frame, and outputs position and velocity vectors, and
			//only the position vector is saved to the vector variable
			//To get coordinates for the moon instead, simply replace "sun" in the below line with "moon"
			Vector3D vector = moon.getPVCoordinates(date, FramesFactory.getITRF(ITRFVersion.ITRF_2014, IERSConventions.IERS_2010, true)).getPosition();
			
			//Gets the x, y, and z coordinates of the position vector and formats them in terms of scientific notation
			String xvector = Double.toString(vector.getX());
			xvector = xvector.replace('E', 'e');
			
			String yvector = Double.toString(vector.getY());
			yvector = yvector.replace('E', 'e');
			
			String zvector = Double.toString(vector.getZ());
			zvector = zvector.replace('E', 'e');
			
			int row = 1;
			
			//Writes the date, time, and position vectors to a StringBuilder and then the string in the StringBuilder is written into the csv file
			//Each line is of the form:
			//id,<row number>,<Year>,<Month Number>,<Day of Month>,<Hour>,<Minute>,<Second>,<X-coordinate>,<Y-coordinate>,<Z-coordinate>
			StringBuilder sb = new StringBuilder();
			sb.append("id,");
			sb.append(Integer.toString(row));
			sb.append(",");
			sb.append(datetime.getYear());
			sb.append(",");
			sb.append(datetime.getMonthValue());
			sb.append(",");
			sb.append(datetime.getDayOfMonth());
			sb.append(",");
			sb.append(datetime.getHour());
			sb.append(",");
			sb.append(datetime.getMinute());
			sb.append(",");
			sb.append((double) datetime.getSecond());
			sb.append(",");
			sb.append(xvector);
			sb.append(",");
			sb.append(yvector);
			sb.append(",");
			sb.append(zvector);
			sb.append("\n");
			
			//System.out.println(sb.toString());
			writer.write(sb.toString());
			
			sb.setLength(0);
			
			row++;
			
			//Loops the entire block above for another 17,543 times. Total number of lines written to the csv file is 17,544 - the total number of
			//hours in 2019 and 2020. To get a longer or shorter csv file, simply change the "17543" below to the amount of desired hours
			//the program should take coordinates for
			//The program increments in hours, and that can be changed if so needed
			for(int i = 0; i < 17543; i++) {
				datetime = datetime.plusHours(1);
				date = new AbsoluteDate(datetime.getYear(), 
						datetime.getMonthValue(), 
						datetime.getDayOfMonth(), 
						datetime.getHour(), 
						datetime.getMinute(), 
						(double) datetime.getSecond(), 
						TimeScalesFactory.getUTC());
				
				vector = moon.getPVCoordinates(date, FramesFactory.getITRF(ITRFVersion.ITRF_2014, IERSConventions.IERS_2010, true)).getPosition();
				
				xvector = Double.toString(vector.getX());
				xvector = xvector.replace('E', 'e');
				
				yvector = Double.toString(vector.getY());
				yvector = yvector.replace('E', 'e');
				
				zvector = Double.toString(vector.getZ());
				zvector = zvector.replace('E', 'e');
				
				sb.append("id,");
				sb.append(Integer.toString(row));
				sb.append(",");
				sb.append(datetime.getYear());
				sb.append(",");
				sb.append(datetime.getMonthValue());
				sb.append(",");
				sb.append(datetime.getDayOfMonth());
				sb.append(",");
				sb.append(datetime.getHour());
				sb.append(",");
				sb.append(datetime.getMinute());
				sb.append(",");
				sb.append((double) datetime.getSecond());
				sb.append(",");
				sb.append(xvector);
				sb.append(",");
				sb.append(yvector);
				sb.append(",");
				sb.append(zvector);
				sb.append("\n");
				
				writer.write(sb.toString());
				
				sb.setLength(0);
				
				row++;
			}
			
			System.out.println("Complete");
			
			
		} catch(FileNotFoundException e) {
			System.out.println(e.getMessage());
		}

	}

}
