import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model: any = {};

  constructor(public authservice: AuthService, private alertify: AlertifyService, 
    private router: Router) { }

  ngOnInit() {
  }

  login(){
    this.authservice.login(this.model).subscribe(
      next => {
        this.alertify.Success('Logged in Succesfully');
      }, error => {
        this.alertify.Error(error);
      }, () => {
        this.router.navigate(['/members'])
      });
  }
  loggedIn() {
    return this.authservice.loggedIn();
  }
  logout() {
    localStorage.removeItem('token');
    this.alertify.Message('logged Out');
    this.router.navigate(['/home']);
  }
}
