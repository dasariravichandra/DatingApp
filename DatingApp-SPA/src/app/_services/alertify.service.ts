import { Injectable } from '@angular/core';
import * as alertify from 'alertifyjs';

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {

constructor() { }

confirm(message: string, okCallback: () => any){
  alertify.confirm(message, (e: any) => {
      if (e) {
        okCallback();
      }
      else {

      }
    });
  }
  Success(message: string){
    alertify.success(message);
  }
  Error(message: string){
    alertify.error(message);
  }
  Warning(message: string){
    alertify.warning(message);
  }
  Message(message: string){
    alertify.success(message);
  }

}
