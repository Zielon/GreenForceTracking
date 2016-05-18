package pl.edu.pg.eti.greenforcetrackingklient.SocketCommunication;

import android.util.Log;
import android.os.Looper;
import android.os.Handler;
import java.io.BufferedWriter;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.net.UnknownHostException;
import java.util.ArrayList;
import java.util.List;

/**
 * Created by Lemur on 2016-04-09.
 */
public class SocketCommunication {

    private static Socket socket;
    private static int port;
    private static String serverIp;
    private List<eventListener> eventListners;
    private connectionHandler connectListner;
    private connectionProblemHandler connectionProblemListner;
    private disconnectionHandler disconnectListner;

    public SocketCommunication(){
        port = 0;
        serverIp = "";
        this.connectListner = null;
        this.disconnectListner = null;
        this.eventListners = new ArrayList<eventListener>();
    }
    public void connect(String ip, int port){
        this.port = port;
        serverIp = ip;
        new Thread(new SocketThread()).start();
        Log.d("GFT", "start listen");
    }
    //handlers for events
    public interface eventHandler {
        public void event(String data);
    }
    public interface connectionHandler {
        public void onConnect();
    }
    public interface connectionProblemHandler {
        public void onConnectionProblem();
    }
    public interface disconnectionHandler {
        public void onDisconnect();
    }
    //declarations of events
    public void addEventListner(String name, eventHandler listener) {
        eventListners.add(new eventListener(name, listener));
    }
    public void onConnect(connectionHandler listener) {
        this.connectListner = listener;
    }
    public void onConnectionProblem(connectionProblemHandler listener) {
        this.connectionProblemListner = listener;
    }
    public void onDisconnect(disconnectionHandler listener) {
        this.disconnectListner = listener;
    }
    //callback on connectimg to functions in main thread
    private void eventConnect(){
        if(this.connectListner!=null){
            Handler handle = new Handler (Looper.getMainLooper());
            handle.post(new Runnable() {
                @Override
                public void run() {
                    connectListner.onConnect();
                }
            });
        }
    }
    //callback on connection problem to functions in main thread
    private void eventConnectionProblem(){
        if(this.connectListner!=null){
            Handler handle = new Handler (Looper.getMainLooper());
            handle.post(new Runnable() {
                @Override
                public void run() {
                    connectionProblemListner.onConnectionProblem();
                }
            });
        }
    }
    //callback on disconnect to functions in main thread
    private void eventDisconnect(){
        if(this.disconnectListner!=null){
            Handler handle = new Handler (Looper.getMainLooper());
            handle.post(new Runnable() {
                @Override
                public void run() {
                    disconnectListner.onDisconnect();
                }
            });
        }
    }
    public void send(String message){
        if(socket!=null) {
            try {
                PrintWriter out = new PrintWriter(new BufferedWriter(new OutputStreamWriter(socket.getOutputStream())), true);
                /*
                socket.getOutputStream - Returns an output stream for this socket.
                OutputStreamWriter - bridge from character streams to byte streams
                BufferedWriter - Writes text to a character-output stream, buffering characters so as to provide for the efficient writing of single characters, arrays, and strings.
                PrintWriter - Prints formatted representations of objects to a text-output stream.
                */
                out.println(message);
                Log.d("GFT", "wyslano");
            } catch (UnknownHostException e) {
                Log.e("GFT", "exception sending", e);
                eventDisconnect();
            } catch (IOException e) {
                Log.e("GFT", "exception sending", e);
                eventDisconnect();
            } catch (Exception e) {
                Log.e("GFT", "exception sending", e);
                eventDisconnect();
            }
        }
    }
    class SocketThread implements Runnable {
        @Override
        public void run() {
            Log.d("GFT", "connecting");
            try {
                socket = new Socket();
                socket.connect(new InetSocketAddress(serverIp, port), 1000);
                new Thread(new SocketListenerThread()).start();
                Log.d("GFT", "connected");
            } catch (UnknownHostException e) {
                Log.e("GFT", "exception connection", e);
                eventConnectionProblem();
            } catch (IOException e) {
                Log.e("GFT", "exception connection", e);
                eventConnectionProblem();
            }
        }
    }
    class SocketListenerThread implements Runnable {
        @Override
        public void run() {
            String msg;
            try {
                BufferedReader in = new BufferedReader(new InputStreamReader(socket.getInputStream()));
                while ((msg = in.readLine()) != null) {
                    Log.d("GFT", "msg: " + msg);
                }
            } catch (IOException e) {
                Log.e("GFT", "exception listening", e);
                eventDisconnect();
            }
        }
    }
    class eventListener {
        public String name;
        public eventHandler handler;
        public eventListener(String name, eventHandler handler){
            this.name = name;
            this.handler = handler;
        }
    }
}