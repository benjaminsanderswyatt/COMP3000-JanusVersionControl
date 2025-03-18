import React from 'react';

import Page from '../../components/Page';
import Card from '../../components/cards/Card';

import styles from "../../styles/pages/legal/Legal.module.css";

const headerSection = (pageStyles) => { return(
  <header className={pageStyles.header}>
    <div className={styles.header}>
      <h3>Terms of Use for Janus Version Control System</h3>
      <p><strong>Last Updated:</strong> 18/03/2025</p>
    </div>
  </header>
)};



const TermsOfUse = () => (
  <Page header={headerSection}>

    <Card>
      <div className={styles.holder}>
        <section>
          <h4>1. Introduction</h4>
          <p>
            Welcome to Janus, a Version Control System (“the Service”) provided by Janus. These Terms of Use (“Terms”) govern your access to and use of Janus. By logging into and using the Service, you agree to be bound by these Terms. If you do not agree with any part of these Terms, you must not use the Service.
          </p>
        </section>

        <section>
          <h4>2. Acceptance of Terms</h4>
          <p>
            By accessing or using Janus, you affirm that you are authorized by your organization to do so and that you agree to comply with and be legally bound by these Terms. Your continued use of the Service constitutes your acceptance of any modifications to these Terms.
          </p>
        </section>
        
        
        <section>
          <h4>3. Description of the Service</h4>
          <p>
            Janus is an on-premise hosted web-based version control system that facilitates source code management and collaboration among authorized users within your organization. The Service may be updated or modified at any time at the discretion of Janus.
          </p>
        </section>
        
        <section>
          <h4>4. User Accounts and Registration</h4>
          <ul>
            <li><strong>Account Creation:</strong> Access to Janus is limited to authorized users who have been provided with account credentials by Janus.</li>
            <li><strong>Responsibility for Accounts:</strong> You are responsible for maintaining the confidentiality of your login credentials. You must immediately notify your organization’s IT department if you suspect any unauthorized use of your account.</li>
            <li><strong>Prohibited Activities:</strong> Users agree not to share their credentials or allow unauthorized individuals access to the Service.</li>
          </ul>
        </section>
        
        <section>
          <h4>5. Acceptable Use</h4>
          <ul>
            <li><strong>Compliance:</strong> You agree to use Janus only for lawful purposes and in accordance with your organization’s policies.</li>
            <li><strong>Prohibited Conduct:</strong> Users must not engage in activities that could harm the Service or its users, including unauthorized data access, system interference, or any form of abuse.</li>
            <li><strong>Monitoring:</strong> Janus reserves the right to monitor usage and investigate any activities that may violate these Terms.</li>
          </ul>
        </section>
        
        <section>
          <h4>6. Intellectual Property</h4>
          <ul>
            <li><strong>Ownership:</strong> All software, documentation, and content provided as part of Janus are the property of Janus or its licensors and are protected by applicable intellectual property laws.</li>
            <li><strong>Limited License:</strong> Your use of the Service is limited to the rights granted by your organization. You agree not to copy, modify, distribute, or reverse engineer any part of the Service without explicit authorization.</li>
          </ul>
        </section>
        
        <section>
          <h4>7. Privacy and Data Protection</h4>
          <p>
            Your use of Janus is subject to your organization’s Privacy Policy and any applicable data protection laws. Janus will collect and use data in accordance with these policies and legal requirements.
          </p>
        </section>
        
        <section>
          <h4>8. Security and User Responsibilities</h4>
          <ul>
            <li><strong>Security Measures:</strong> While Janus implements security measures to protect the Service, you are responsible for following best practices regarding password security and data handling.</li>
            <li><strong>User Data:</strong> You must not upload, store, or share any data that may compromise the security of the Service or the privacy of other users.</li>
          </ul>
        </section>
        
        <section>
          <h4>9. Disclaimer of Warranties</h4>
          <p>
            Janus is provided on an “as is” and “as available” basis. Janus disclaims all warranties, whether express or implied, including but not limited to warranties of merchantability, fitness for a particular purpose, and non-infringement. Use of the Service is at your own risk.
          </p>
        </section>
        
        <section>
          <h4>10. Limitation of Liability</h4>
          <p>
            In no event shall Janus be liable for any indirect, incidental, special, or consequential damages arising out of or related to your use or inability to use Janus, even if advised of the possibility of such damages. The total liability of Janus, whether in contract, tort, or otherwise, shall not exceed the fees paid (if any) for the Service.
          </p>
        </section>
        
        <section>
          <h4>11. Indemnification</h4>
          <p>
            You agree to indemnify and hold harmless Janus, its officers, employees, and affiliates from any claims, damages, or expenses arising from your use of Janus, your violation of these Terms, or your infringement of any intellectual property or other rights of any third party.
          </p>
        </section>
        
        <section>
          <h4>12. Modifications to the Terms</h4>
          <p>
            Janus reserves the right to amend or update these Terms at any time. You will be notified of material changes as required by your organization’s policies. Continued use of the Service following such changes constitutes your acceptance of the updated Terms.
          </p>
        </section>

        <section>
        <h4>13. Governing Law and Jurisdiction</h4>
          <p>
            These Terms shall be governed by and construed in accordance with the law. Any disputes arising from or relating to these Terms or your use of Janus will be resolved in the appropriate court.
          </p>
        </section>

        <section>
          <h4>14. Termination</h4>
          <p>
            Janus reserves the right to terminate or suspend your access to Janus at any time, with or without notice, for conduct that violates these Terms or for any other reason deemed necessary by your organization.
          </p>
        </section>

        <section>
          <h4>15. Miscellaneous</h4>
          <ul>
            <li><strong>Severability:</strong> If any provision of these Terms is held to be invalid or unenforceable, such provision will be struck and the remaining Terms will remain in full force and effect.</li>
            <li><strong>Entire Agreement:</strong> These Terms, together with any additional policies or guidelines provided by Janus, constitute the entire agreement between you and Janus regarding the use of Janus.</li>
          </ul>
        </section>
        
        <section>
          <h4>16. Contact Information</h4>
          <p>
            For any questions regarding these Terms or the Service, please contact Janus at:
          </p>
          <p>
            Email: office@Janus.com
          </p>
        </section>
      </div>
    </Card>

  </Page>
);

export default TermsOfUse;
