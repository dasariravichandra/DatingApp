import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';


@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

@Output() cancelRegister = new EventEmitter();
  model: any = {};
  constructor(private authService: AuthService) { }

  ngOnInit() {
  }
  register(){
    this.authService.Register(this.model).subscribe(()=>{
      console.log('register Succesful');
    }, error=> {
      console.log(error);
    });
  }
  cancel(){
      this.cancelRegister.emit(false);
      console.log('cancelled');
  }
}
