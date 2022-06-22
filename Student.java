
package StudentApplication;

/**
 * Object class
 * @author Sai
 */
public class Student 
{
    private String firstName;
    private String lastName;
    
    private long studentID;
    
    private int dobDay;
    private int dobMonth;
    private int dobYear;
    
    /**
     * Default Constructor
     */
    public Student()
    {
        firstName = "";
        lastName = "";       
        studentID = 0;        
        dobDay = dobMonth = dobYear = 0;
    }
    
    /**
     * Constructor with values
     * @param firstName first name of the student
     * @param lastName last name of the student
     * @param studentID student id number
     * @param dobD day on which student was born
     * @param dobM month on which student was born
     * @param dobY year on which student was born
     */
    public Student(String firstName, String lastName, long studentID, int dobD, int dobM, int dobY)
    {
        this.firstName = firstName;
        this.lastName = lastName;
        this.studentID = studentID;
        dobDay = dobD;
        dobMonth = dobM;
        dobYear = dobY;
    }
    
    /**
     * Used to get name
     * @return first name and last name
     */
    public String getName()
    {
        return firstName + " " + lastName;
    }
    
    /**
     * Used to get date of birth
     * @return returns date of birth in d/m/y format
     */
    public String getDOB()
    {
        return + dobDay + "/" + dobMonth + "/" + dobYear;
    }
    
    /**
     * Used to get student id number
     * @return studentID
     */
    public long getStudentID()
    {
        return studentID;
    }
    
    /**
     * Used to calculate final grade
     * @param overallMark overall mark of the student
     * @return final grade as a string
     */
    public String calcFinalGrade(int overallMark)
    {
        if (overallMark >= 80)
        {
            return "HD";
        }
        else if (overallMark >= 70)
        {
           return "D";
        }
        else if (overallMark >= 60)
        {
            return "C";
        }
        else if (overallMark >= 50)
        {
            return "P";
        }
        else 
        {
            return "N";
        }
    }
    
    /**
     * Used to get overall mark
     * @return null for parent class
     */
    public int getOverallMark()
    {
        return 0;
    }
    
    /**
     * Used to get final grade
     * @return null for parent class
     */
    public String getFinalGrade()
    {
        return null;
    }
    
    /**
     * Used to check if 2 students are the same
     * @param s student object
     * @return true or false based on if student is the same as student s
     */
    public boolean isEqual(Student s)
    {
        if (this.getName().equals(s.getName()) && this.getDOB().equals(s.getDOB()))
        {
            return true;
        }
        
        return false;
    }
    
    /**
     * To string method for class
     * @return Name, Student ID, DOB
     */
    public String toString()
    {
        return " Name: " + firstName + " " + lastName + " Student ID: " + studentID + " DOB: " + dobDay + "/" + dobMonth + "/" + dobYear;
    }
}
