package pl.edu.pg.eti.greenforcetrackingklient;

import android.content.Intent;
import android.app.Activity;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.widget.Button;
import android.view.View;
import android.widget.EditText;
import pl.edu.pg.eti.greenforcetrackingklient.ListOfRooms.ListOfRoomsActivity;

public class WelcomeActivity extends Activity {
    EditText loginField;
    EditText passwordField;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        loginField = (EditText)  findViewById(R.id.loginInput);
        passwordField = (EditText)  findViewById(R.id.passwordInput);
        Button clickButton = (Button) findViewById(R.id.button);
        clickButton.setOnClickListener(new View.OnClickListener() {
            public void onClick(View v) {
                String nameText = loginField.getText().toString();
                String passwordText = passwordField .getText().toString();
                Bundle dataBundle = new Bundle();
                dataBundle.putString("login", nameText);
                dataBundle.putString("password", passwordText);
                Intent joiningIntend = new Intent(getBaseContext(), ListOfRoomsActivity.class);
                joiningIntend.putExtras(dataBundle);
                startActivity(joiningIntend);
            }
        });
        new DialogAlert("Error", "Connection can't be established", this);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            return true;
        }

        return super.onOptionsItemSelected(item);
    }

    public void onConnect(){

    }
}
