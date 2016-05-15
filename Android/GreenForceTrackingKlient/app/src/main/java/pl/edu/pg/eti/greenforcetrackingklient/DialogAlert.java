package pl.edu.pg.eti.greenforcetrackingklient;

import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Context;
import android.content.DialogInterface.OnDismissListener;
import android.app.Activity;

/**
 * Created by Lemur on 2016-04-23.
 */
public class DialogAlert {
    public DialogAlert(String title, String text, Context context){
        AlertDialog.Builder builder = new AlertDialog.Builder(context);
        builder.setTitle(title);
        builder.setMessage(text);
        builder.setCancelable(true);
        builder.setNeutralButton(android.R.string.ok,
                new DialogInterface.OnClickListener() {
                    public void onClick(DialogInterface dialog, int id) {

                        dialog.cancel();
                    }
                });
        AlertDialog alert = builder.create();
        alert.show();
    }
}
