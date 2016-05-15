package pl.edu.pg.eti.greenforcetrackingklient.ListOfRooms;

/**
 * Created by Lemur on 2016-04-09.
 */
import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.util.Log;
import pl.edu.pg.eti.greenforcetrackingklient.R;
import pl.edu.pg.eti.greenforcetrackingklient.SocketCommunication.SocketCommunication;

public class ListOfRoomsActivity extends Activity {
    public static SocketCommunication communication;
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.rooms_list_layout);
        Bundle extras = getIntent().getExtras();    //reading username from bundle
        TextView text = (TextView)findViewById(R.id.userName);
        String login = "";
        String password = "";
        if(extras != null) {
            login = extras.getString("login");
            password = extras.getString("password");
            text.setText("user: " + login + "/" + password);
        }
        communication = new SocketCommunication();
        communication.onConnect(new SocketCommunication.connectionHandler() {
            @Override
            public void onConnect() {
                Log.d("GFT", "connected");
            }
        });
        communication.onDisconnect(new SocketCommunication.disconnectionHandler() {
            @Override
            public void onDisconnect() {
                Log.d("GFT", "disconnected");
                finish();
            }
        });
        communication.onConnectionProblem(new SocketCommunication.connectionProblemHandler(){
            @Override
            public void onConnectionProblem(){
                Log.d("GFT", "connection problem");
                finish();
            }
        });
        communication.connect("kornik.ddns.net", 52300);

        Button clickButton = (Button) findViewById(R.id.enterRoomButton);
        clickButton.setOnClickListener(new View.OnClickListener() {
            public void onClick(View v) {
                communication.send("<Player><ID>1234</ID><User>Tesownik</User><Lat>123.234</Lat><Lon>123.234</Lon><Message>Test</Message></Player>");
                //startActivity(new Intent(getBaseContext(), RoomActivity.class));
            }
        });
    }
}
