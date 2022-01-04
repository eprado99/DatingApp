import { THIS_EXPR } from '@angular/compiler/src/output/output_ast';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccountService } from '../services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter<boolean>();
  registerModel: any = {};
  constructor(private accountService: AccountService) { }

  ngOnInit(): void { }

  register(){
    this.accountService.register(this.registerModel).subscribe(response => {
      console.log(response);
      this.cancel();
    }, err => {
      console.log(err);
    });
    console.log(this.registerModel);
  }

  cancel(){
    this.cancelRegister.emit(false);
    console.log("cancelled");
  }
}
